/*  AUTHOR          :   FELIX ENGLAND
 *  VERSION         :   V1.0 5/10/2018
 *  
 *  Form1.cs
 *  PURPOSE         :   Main code for the userinterface
 *  LAST MODIFIED   :   13/10/2018
 *  MODIFIED BY     :   FELIX ENGLAND
 *  NOTES           :   
 *  
 *  TODO            :   Fix the crazy lag that you get when you zoom in and out multiple times
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Web;

namespace HFBespoke
{
    public partial class Form1 : Form
    {
        //Used for saving the data to an XML 
        public class CircleButtonSerialise
        {
            public string pageLink { get; set; }
            public int radius { get; set; }
            public Point centre { get; set; }
        }

        public class CustomButtonSerialise
        {
            public int btnType { get; set; }
            public string pageLink { get; set; }
            public Point[] poly { get; set; }
        }

        public class DisplayPageSerialise
        {
            public string name { get; set; }
            public string backgroundImage { get; set; }
            public List<CustomButtonSerialise> buttonList { get; set; }
            public List<CircleButtonSerialise> circleBtnLst { get; set; }
        }

        public class NewTabPage
        {
            public string tabName { get; set; }
            public double percentageOfZoom { get; set; }
            public GroupBox grpBox { get; set; }
            public PictureBox picBox { get; set; }
        }
        //--------------------------------------------------------------------------

        private Size projectDisplaySize;
        private List<DisplayPage> pageLst;
        private List<Timer> timerLst; //This list is used to stop timers ticking when the page is not in use
        private List<NewTabPage> tabLst;
        private List<RightClickContextMenu> cmLst; //Used to get the point when the context menu is opened
        private DateTime? lastSave = null;
        private bool? isCreatingButton = null;
        private bool autoSave = false;
        private int currentButtonType = 0;
        private string saveFolder = @"";
        int r;

        public Form1()
        {
            InitializeComponent();
            pageLst = new List<DisplayPage>();
            timerLst = new List<Timer>();
            tabLst = new List<NewTabPage>();
            cmLst = new List<RightClickContextMenu>();
            tabControlDesignerView.SelectedIndexChanged += new EventHandler(Tabs_SelectedIndexChanged);
            this.WindowState = FormWindowState.Maximized;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            //If the images folder doesn't exist create one
            //This is done because if I load all the images into the memory I get out of memory errors
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\images"))
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\images");

            else
            {
                DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\images");

                if (di.GetFiles().Length > 0)
                {
                    foreach (FileInfo file in di.EnumerateFiles())
                        file.Delete();

                    foreach (DirectoryInfo dir in di.EnumerateDirectories())
                        dir.Delete(true);
                }
            }
        }

        private void projectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewProject();
        }

        /// <summary>
        /// completely clears all the pages open and creates a new project
        /// </summary>
        private void CreateNewProject()
        {
            if (pageLst.Count > 0)
            {
                DialogResult discard = MessageBox.Show("Do you want to discard your old project?",
                    "Discard Old Project",
                    MessageBoxButtons.YesNo);

                if (discard == DialogResult.Yes)
                {
                    tabControlDesignerView.TabPages.Clear();
                    projectDisplaySize = GetProjectSize();
                    CreateNewTab("Main Page", GetPathForImage(), false);
                }
            }

            else
            {
                tabControlDesignerView.TabPages.Clear();
                projectDisplaySize = GetProjectSize();
                CreateNewTab("Main Page", GetPathForImage(), false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Size GetProjectSize()
        {
            try
            {
                return new Size(Convert.ToInt32(Interaction.InputBox("Please input the width of the display", "Width of screen")),
                Convert.ToInt32(Interaction.InputBox("Please input the height of the display", "Height of screen")));
            }

            catch (Exception ex)
            {
                MessageBox.Show("Please enter numbers only");
                return GetProjectSize();
            }
        }

        //Needs to be made so that you can only create a new page when a proect has already been created
        private void pageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (projectDisplaySize != new Size(0, 0))
                CreateNewTab(Interaction.InputBox("Please input a name for the tab", "Name of tab"), GetPathForImage(), false);
            else
                MessageBox.Show("Please Create a Project First");
        }

        private void projectFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetAllVars();

            string imagesPath = "";
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    imagesPath = fbd.SelectedPath;
            }

            try
            {
                Image main = Image.FromFile(imagesPath + @"\Main Page.png");
                int screenWidth = main.Width; int screenHeight = main.Height;
                projectDisplaySize = new Size(screenWidth, screenHeight);

                DirectoryInfo di = new DirectoryInfo(imagesPath);
                FormatString format = new FormatString();

                foreach (var v in di.GetFiles())
                    CreateNewTab(format.Turnicate(v.Name, (v.Name.Length - 4)), v.Directory + @"\" + v.Name, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not find Main Page.png, please make sure this is in the folder before attempting to load",
                    "Project load error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFolder == @"")
            {
                SaveFileDialog saveDi = new SaveFileDialog();
                saveDi.Filter = "hen Files|*.hen";
                saveDi.Title = "Save project file";
                saveDi.ShowDialog();

                if (!string.IsNullOrWhiteSpace(saveDi.FileName))
                    saveFolder = saveDi.FileName;
                else
                    MessageBox.Show("Please enter a name for the file",
                    "Project save error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }

            Save();
        }

        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlDesignerView.SelectedTab != null)
                foreach (Timer t in timerLst)
                {
                    if ((string)t.Tag == tabControlDesignerView.SelectedTab.Text)
                        t.Enabled = true;
                    else
                        t.Enabled = false;
                }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDi = new SaveFileDialog();
            saveDi.Filter = "hen Files|*.hen";
            saveDi.Title = "Save project file";
            saveDi.ShowDialog();

            if (!string.IsNullOrWhiteSpace(saveDi.FileName))
                saveFolder = saveDi.FileName;
            else
                MessageBox.Show("Please enter a name for the file",
                "Project save error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);

            Save();
        }

        private void autoSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFolder == @"")
            {
                SaveFileDialog saveDi = new SaveFileDialog();
                saveDi.Filter = "hen Files|*.hen";
                saveDi.Title = "Save project file";
                saveDi.ShowDialog();

                if (!string.IsNullOrWhiteSpace(saveDi.FileName))
                    saveFolder = saveDi.FileName;
                else
                    MessageBox.Show("Please enter a name for the file",
                    "Project save error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }

            Save();
            autoSave = true;
            lastSave = DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Save()
        {
            List<DisplayPageSerialise> displayLstS = new List<DisplayPageSerialise>();
            XMLConverter xml = new XMLConverter();
            //Directory.CreateDirectory(saveFolder + @"\images");

            foreach (DisplayPage d in pageLst)
            {
                DisplayPageSerialise dS = new DisplayPageSerialise();
                dS.name = d.displayPageName;
                dS.backgroundImage = d.GetByteArrayFromBachgroundImage();
                dS.buttonList = new List<CustomButtonSerialise>();
                dS.circleBtnLst = new List<CircleButtonSerialise>();

                if (d.GetButtonList().Count > 0)
                {
                    foreach (CustomButton c in d.GetButtonList())
                    {
                        CustomButtonSerialise cS = new CustomButtonSerialise();
                        cS.btnType = c.GetButtonType();
                        cS.pageLink = c.GetPageLink();
                        cS.poly = c.GetPoly();
                        dS.buttonList.Add(cS);
                    }
                }

                if (d.GetCircleBtnLst().Count > 0)
                {
                    foreach (CircleButton c in d.GetCircleBtnLst())
                    {
                        CircleButtonSerialise cS = new CircleButtonSerialise();
                        cS.centre = c.GetCentre();
                        cS.pageLink = c.GetPageLink();
                        cS.radius = c.GetRadius();
                        dS.circleBtnLst.Add(cS);
                    }
                }

                displayLstS.Add(dS);
            }
            File.WriteAllText(saveFolder, xml.ToXml(displayLstS));
        }

        /// <summary>
        /// Asks the user to select an image then returns the path
        /// </summary>
        /// <returns></returns>
        private string GetPathForImage()
        {
            string imgPath = "";
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "png files (*.png)|*.png";

                if (dlg.ShowDialog() == DialogResult.OK)
                    imgPath = dlg.FileName;
            }
            return imgPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabName"></param>
        /// <param name="imgPath"></param>
        /// <param name="fromFile"></param>
        private void CreateNewTab(string tabName, string imgPath, bool fromFile)
        {
            GroupBox tabGroupBox = new GroupBox();
            tabGroupBox.Size = new Size(projectDisplaySize.Width + 100, projectDisplaySize.Height + 100);
            tabGroupBox.Location = new Point(((tabControlDesignerView.Width / 2) - (tabGroupBox.Width / 2)), 50);
            tabGroupBox.Text = "Designer View";
            tabGroupBox.Anchor = AnchorStyles.Top; //Makes the groupBox relocate when the form is resized by the user

            PictureBox designerViewPage = new PictureBox();
            designerViewPage.Size = projectDisplaySize; //The height and width need to be formatted so they will fit within the window but the aspect ratio needs to stay the same
            designerViewPage.Location = new Point(50, 50);
            designerViewPage.MouseClick += new MouseEventHandler(PictureBox_Click);
            designerViewPage.Name = tabName; //This is for when the user clicks on the picture box I don't have to try and get the tab name
            designerViewPage.BackgroundImageLayout = ImageLayout.Stretch;

            RightClickContextMenu picBoxCM = new RightClickContextMenu();
            picBoxCM.MenuItems.Add("Delete Button", new EventHandler(DeleteButton_Click));
            picBoxCM.MenuItems.Add("Alter Page Link", new EventHandler(AlterButton_Click));
            picBoxCM.Name = tabName;
            cmLst.Add(picBoxCM);

            Timer buttonDrawing = new Timer();
            buttonDrawing.Interval = 10; //Interval is 10 maybe change this so that it is less CPU intensive
            buttonDrawing.Enabled = true;
            buttonDrawing.Tag = tabName;
            buttonDrawing.Tick += new EventHandler((sender, e) => Timer_Tick(sender, e, designerViewPage, imgPath));
            timerLst.Add(buttonDrawing);

            TabPage tabPage = new TabPage();
            tabPage.Text = tabName;
            tabPage.AutoScroll = true;

            designerViewPage.ContextMenu = picBoxCM;
            tabGroupBox.Controls.Add(designerViewPage);
            tabControlDesignerView.TabPages.Add(tabPage);
            tabPage.Controls.Add(tabGroupBox);

            //Might want to improve this
            NewTabPage newTab = new NewTabPage();
            newTab.tabName = tabName;
            newTab.picBox = designerViewPage;
            newTab.grpBox = tabGroupBox;
            newTab.percentageOfZoom = 100f;
            tabLst.Add(newTab);

            if (fromFile == false)
            {
                DisplayPage newDP = new DisplayPage(tabName);
                newDP.AddImagePath(imgPath);
                pageLst.Add(newDP);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            //The code in this function is discgusting and almost getting to the point of spaghetti code
            //Shits getting hard to keep track of and needs to be optimised
            //Grabs the point on the picturebox the right click was made
            RightClickContextMenu rcm = new RightClickContextMenu();
            PictureBox pic = new PictureBox();
            double percentageOfZoom = 0;
            double xCoordinate = 0;
            double yCoordinate = 0;

            foreach (NewTabPage t in tabLst)
                if (t.tabName == tabControlDesignerView.SelectedTab.Text)
                {
                    pic = t.picBox;
                    percentageOfZoom = t.percentageOfZoom;
                    break;
                }

            foreach (RightClickContextMenu r in cmLst)
                if (r.Name == tabControlDesignerView.SelectedTab.Text)
                {
                    xCoordinate = r.p.X;
                    yCoordinate = r.p.Y;
                    break;
                }

            //Maybe I should put this in its own fuction seen as it's repeated so many times
            var screenPosition = pic.PointToScreen(new Point(0, 0));
            Point p = new Point((int)((xCoordinate - screenPosition.X) / (percentageOfZoom / 100)), (int)((yCoordinate - screenPosition.Y) / (percentageOfZoom / 100)));

            DetectIfPointIsInside detectIfPoint = new DetectIfPointIsInside(projectDisplaySize.Width);

            if (p != null && p != new Point(0, 0))
            {
                foreach (DisplayPage d in pageLst)
                    if (d.displayPageName == tabControlDesignerView.SelectedTab.Text)
                    {
                        foreach (CustomButton c in d.GetButtonList())
                            if (detectIfPoint.DoesIntersect(c.GetPoly(), c.GetPoly().Length, p))
                            {
                                d.DeleteCustomButton(c.GetPoly());
                                break;
                            }

                        foreach (CircleButton c in d.GetCircleBtnLst())
                            if (detectIfPoint.DoesIntersectCircle(c.GetRadius(), p, c.GetCentre()))
                            {
                                d.DeleteCircleButton(c.GetCentre(), c.GetRadius());
                                break;
                            }
                        break;
                    }
            }

            else
                MessageBox.Show("Error could not find the point the right click was made",
                "INTERNAL ERROR",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
        }

        private void AlterButton_Click(object sender, EventArgs e)
        {
            //The code in this function is discgusting and almost getting to the point of spaghetti code
            //Shits getting hard to keep track of and needs to be optimised
            //Grabs the point on the picturebox the right click was made
            RightClickContextMenu rcm = new RightClickContextMenu();
            PictureBox pic = new PictureBox();
            double percentageOfZoom = 0;
            double xCoordinate = 0;
            double yCoordinate = 0;

            foreach (NewTabPage t in tabLst)
                if (t.tabName == tabControlDesignerView.SelectedTab.Text)
                {
                    pic = t.picBox;
                    percentageOfZoom = t.percentageOfZoom;
                    break;
                }

            foreach (RightClickContextMenu r in cmLst)
                if (r.Name == tabControlDesignerView.SelectedTab.Text)
                {
                    xCoordinate = r.p.X;
                    yCoordinate = r.p.Y;
                    break;
                }

            //Maybe I should put this in its own fuction seen as it's repeated so many times
            var screenPosition = pic.PointToScreen(new Point(0, 0));
            Point p = new Point((int)((xCoordinate - screenPosition.X) / (percentageOfZoom / 100)), (int)((yCoordinate - screenPosition.Y) / (percentageOfZoom / 100)));

            DetectIfPointIsInside detectIfPoint = new DetectIfPointIsInside(projectDisplaySize.Width);

            if (p != null && p != new Point(0, 0))
            {
                foreach (DisplayPage d in pageLst)
                    if (d.displayPageName == tabControlDesignerView.SelectedTab.Text)
                    {
                        foreach (CustomButton c in d.GetButtonList())
                            if (detectIfPoint.DoesIntersect(c.GetPoly(), c.GetPoly().Length, p))
                            {
                                c.SetPageLink(GetSelectedPage());
                                break;
                            }

                        foreach (CircleButton c in d.GetCircleBtnLst())
                            if (detectIfPoint.DoesIntersectCircle(c.GetRadius(), p, c.GetCentre()))
                            {
                                c.SetPageLink(GetSelectedPage()); ;
                                break;
                            }
                        break;
                    }
            }

            else
                MessageBox.Show("Error could not find the point the right click was made",
                "INTERNAL ERROR",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
        }

        private void PictureBox_Click(object sender, MouseEventArgs e)
        {
            //Used to check how close the click is to the first point
            DetectIfPointIsInside ifPointIsInside = new DetectIfPointIsInside(((PictureBox)sender).Width);
            bool doesIntersect = false;

            //Gets the position of the mouse cursor inside the picturebox
            PictureBox pic = (PictureBox)sender;
            var screenPosition = pic.PointToScreen(new Point(0, 0));
            double percentageOfZoom = 0;

            //Grbs the percentage of zoom from the tab page
            foreach (NewTabPage t in tabLst)
                if (t.tabName == pic.Name)
                {
                    percentageOfZoom = t.percentageOfZoom;
                    break;
                }

            //Maybe I should put this in its own fuction seen as it's repeated so many times
            double xCoordinate = Cursor.Position.X;
            double yCoordinate = Cursor.Position.Y;
            Point cP = new Point((int)((xCoordinate - screenPosition.X) / (percentageOfZoom / 100)), (int)((yCoordinate - screenPosition.Y) / (percentageOfZoom / 100)));

            foreach (DisplayPage d in pageLst)
            {
                if (d.displayPageName == ((PictureBox)sender).Name)
                {
                    foreach (CustomButton b in d.GetButtonList())
                        if (ifPointIsInside.DoesIntersect(b.GetPoly(), b.GetPoly().Length, cP))
                            doesIntersect = true;
                    foreach (CircleButton c in d.GetCircleBtnLst())
                        if (ifPointIsInside.DoesIntersectCircle(c.GetRadius(), cP, c.GetCentre()))
                            doesIntersect = true;
                    //Detects if the new line intersects with any of the others 
                    if (d.GetCurrentButton().Count > 2)
                        for (int i = 0; i < d.GetCurrentButton().Count - 2; i++)
                        {
                            int next = (i + 1) % d.GetCurrentButton().Count;

                            if (ifPointIsInside.doIntersect(d.GetCurrentButton()[i], d.GetCurrentButton()[next], cP, d.GetCurrentButton()[d.GetCurrentButton().Count - 1]))
                            {
                                if (ifPointIsInside.orientation(d.GetCurrentButton()[i], cP, d.GetCurrentButton()[next]) == 0)
                                    doesIntersect = ifPointIsInside.OnSegment(d.GetCurrentButton()[i], cP, d.GetCurrentButton()[next]);
                                else
                                    doesIntersect = true;
                            }
                        }

                    if (!doesIntersect)
                    {
                        //Finds if the click is within 5px of the first point 
                        //If true, the button is complete
                        if (isCreatingButton == true)
                            if (d.GetCurrentButton().Count != 0)
                            {
                                if (!ifPointIsInside.DoesIntersectCircle(5, cP, d.GetCurrentButton()[0]))
                                    d.AddPointToCurrentButton(cP);
                                else
                                {
                                    isCreatingButton = false;
                                    btnFinish.Visible = false;
                                    btnCancel.Visible = false;
                                    break;
                                }
                            }

                            else
                            {
                                d.AddPointToCurrentButton(cP);
                                break;
                            }

                        if (isCreatingButton == false)
                        {
                            if (d.displayPageName == ((PictureBox)sender).Name)
                                d.FinishButton(currentButtonType, GetSelectedPage());

                            isCreatingButton = null;
                            break;
                        }

                        if (d.displayPageName == ((PictureBox)sender).Name && d.isCreatingCircle == true)
                        {
                            Point c = new Point((int)(cP.X - (r / 2)), (int)(cP.Y - (r / 2)));
                            d.isCreatingCircle = false;
                            d.AddCircleButton(c, r, GetSelectedPage());
                            break;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Buttons cannot intersect",
                                                "Button create error",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error,
                                                MessageBoxDefaultButton.Button1);
                        break;
                    }
                }
            }
        }

        private string GetSelectedPage()
        {
            List<string> tabPages = new List<string>();
            List<TabPage> tabPageLst = new List<TabPage>();

            string res = "";

            foreach (TabPage t in tabControlDesignerView.TabPages)
                tabPageLst.Add(t);

            for (int i = 0; i < tabPageLst.Count; i++)
                if (tabControlDesignerView.SelectedTab.Text != tabPageLst[i].Text) //Makes sure the user can't link a button to the same page the button is on
                    tabPages.Add(tabPageLst[i].Text);

            SelectLinkPage form = new SelectLinkPage(tabPages);

            var result = form.ShowDialog();
            if (result == DialogResult.OK)
                res = form.selectedPage;            //values preserved after close

            return res;
        }

        //Too much is being done here it's starting to lag after zooming in and out multiple times
        private void Timer_Tick(object sender, EventArgs e, PictureBox p, string path)
        {
            Timer thisTimer = (Timer)sender;
            Image img = null; //Needs to be updated so the user can input an image
            double percentageOfZoom = 0;

            foreach (DisplayPage d in pageLst)
                if (d.displayPageName == (string)thisTimer.Tag)
                {
                    img = ResizeImage(d.GetBackgroundImage(), projectDisplaySize.Width, projectDisplaySize.Height); // otherwise if the background image was more than this then it wouldn't be able to properly place the buttons
                    break;
                }
            Bitmap bmp = new Bitmap(img);

            if (autoSave == true && lastSave != null && (DateTime.Now - lastSave).Value.TotalSeconds >= 10)
                Save();

            //Grbs the percentage of zoom from the tab page
            foreach (NewTabPage t in tabLst)
                if (t.tabName == p.Name)
                {
                    percentageOfZoom = t.percentageOfZoom;
                    break;
                }

            if (tabControlDesignerView.SelectedTab.Text == p.Name)
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    Pen pen = new Pen(Color.Cyan, 2.0f);

                    foreach (DisplayPage d in pageLst)
                        if (d.displayPageName == p.Name)
                        {
                            foreach (CustomButton cB in d.GetButtonList())
                            {
                                for (int i = 1; i < cB.GetPoly().Count(); i++)
                                    g.DrawLine(pen, cB.GetPoly()[i - 1].X, cB.GetPoly()[i - 1].Y, cB.GetPoly()[i].X, cB.GetPoly()[i].Y);

                                if (cB.GetPoly().Count() <= 0)
                                    break;

                                g.DrawLine(pen, cB.GetPoly()[0].X, cB.GetPoly()[0].Y, cB.GetPoly()[cB.GetPoly().Count() - 1].X, cB.GetPoly()[cB.GetPoly().Count() - 1].Y);
                            }

                            foreach (Point point in d.GetCurrentButton())
                                for (int i = 1; i < d.GetCurrentButton().Count(); i++)
                                    g.DrawLine(pen, d.GetCurrentButton()[i - 1].X, d.GetCurrentButton()[i - 1].Y, d.GetCurrentButton()[i].X, d.GetCurrentButton()[i].Y);

                            foreach (CircleButton c in d.GetCircleBtnLst())
                            {
                                Point centre = c.GetCentre();
                                g.DrawEllipse(pen, centre.X, centre.Y, c.GetRadius(), c.GetRadius());
                            }

                            //Minus r (radius) from the position to the centre
                            if (d.isCreatingCircle == true)
                            {
                                var screenPosition = p.PointToScreen(new Point(0, 0));
                                double xCoordinate = Cursor.Position.X;
                                double yCoordinate = Cursor.Position.Y;
                                Point cP = new Point((int)((xCoordinate - screenPosition.X) / (percentageOfZoom / 100)), (int)((yCoordinate - screenPosition.Y) / (percentageOfZoom / 100)));
                                g.DrawEllipse(pen, (int)(cP.X - (r / 2)), (int)(cP.Y - (r / 2)), r, r);

                            }
                        }
                }

            //Delete the old bitmaps from memory
            GC.Collect();
            p.BackgroundImage = bmp;
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void bntSquare_Click(object sender, EventArgs e)
        {
            if (projectDisplaySize != new Size(0, 0) && pageLst.Count > 0)
            {
                isCreatingButton = true;
                btnFinish.Visible = true;
                currentButtonType = 0;
            }
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            isCreatingButton = false;
        }

        private void btnCircle_Click(object sender, EventArgs e)
        {
            if (projectDisplaySize != new Size(0, 0) && pageLst.Count > 0)
            {
                try
                {
                    r = Convert.ToInt32(Interaction.InputBox("Please input the radius of the circle", "Radius Of Button"));

                    foreach (DisplayPage d in pageLst)
                        if (d.displayPageName == tabControlDesignerView.SelectedTab.Text)
                            d.isCreatingCircle = true;
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Error creating button",
                        "Button creation error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (projectDisplaySize != new Size(0, 0) && pageLst.Count > 0)
            {
                Point p1 = new Point(0, 0);
                Point p2 = new Point(0, 50);
                Point p3 = new Point(70, 50);
                Point p4 = new Point(70, 0);
                Point[] points = new Point[] { p1, p2, p3, p4 };
                CustomButton customButton = new CustomButton(points, 0);
                customButton.SetPageLink(GetSelectedPage());

                foreach (DisplayPage d in pageLst)
                    if (d.displayPageName == tabControlDesignerView.SelectedTab.Text)
                        d.AddCompletedButton(customButton);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            foreach (DisplayPage d in pageLst)
                if (d.displayPageName == tabControlDesignerView.SelectedTab.Text)
                    d.DeleteCurrentButton();

            isCreatingButton = false;
            btnFinish.Visible = false;
            btnCancel.Visible = false;
        }

        private void loadProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult discard = DialogResult.No;
            bool userCurrentlyCreatingProject = true;

            if (projectDisplaySize != new Size(0, 0))
                userCurrentlyCreatingProject = true;

            else
                userCurrentlyCreatingProject = false;

            if (userCurrentlyCreatingProject == true)
            {
                discard = MessageBox.Show("Do you want to discard your old project?",
                        "Discard Old Project",
                        MessageBoxButtons.YesNo);
            }

            if (discard == DialogResult.Yes || userCurrentlyCreatingProject == false)
            {
                XMLConverter xml = new XMLConverter();

                ResetAllVars();

                string filePath = "";
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Open Project";
                    dlg.Filter = "hen files (*.hen)|*.hen";

                    if (dlg.ShowDialog() == DialogResult.OK)
                        filePath = dlg.FileName;
                }

                try
                {
                    string xmlText = File.ReadAllText(filePath);

                    PopulatePageLst(xml.FromXml<DisplayPageSerialise[]>(xmlText));

                    Image main = null;

                    foreach (DisplayPage d in pageLst)
                        if (d.displayPageName == "Main Page")
                            main = d.GetBackgroundImage();

                    if (main != null)
                    {
                        int screenWidth = main.Width; int screenHeight = main.Height;
                        projectDisplaySize = new Size(screenWidth, screenHeight);
                    }

                    foreach (DisplayPage d in pageLst)
                        CreateNewTab(d.displayPageName, d.GetImagePath(), true);
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Error could not load the project",
                        "Project load error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }
        private void ResetAllVars()
        {
            //Reset all the variabes and lists for a new project
            foreach (Timer t in timerLst) //Turns all the timers off
                t.Enabled = false;
            pageLst.Clear();
            timerLst.Clear();
            tabLst.Clear();
            cmLst.Clear();
            isCreatingButton = null;
            autoSave = false;
            lastSave = null;
            currentButtonType = 0;
            saveFolder = @"";
            tabControlDesignerView.TabPages.Clear();
            GC.Collect();
        }

        private void PopulatePageLst(DisplayPageSerialise[] displayPages)
        {
            XMLConverter xml = new XMLConverter();
            foreach (DisplayPageSerialise d in displayPages)
            {
                DisplayPage dS = new DisplayPage(d.name);

                if (d.buttonList.Count > 0)
                {
                    foreach (CustomButtonSerialise c in d.buttonList)
                    {
                        CustomButton cS = new CustomButton(c.poly, 0);
                        cS.SetPageLink(c.pageLink);
                        dS.AddCompletedButton(cS);
                    }
                }

                if (d.circleBtnLst.Count > 0)
                    foreach (CircleButtonSerialise c in d.circleBtnLst)
                        dS.AddCircleButton(c.centre, c.radius, c.pageLink);

                Image bgI = xml.GetImageFromBase64(d.backgroundImage);
                bgI.Save(AppDomain.CurrentDomain.BaseDirectory + @"\images\" + d.name + ".png");

                dS.AddBackgroundImage(bgI);
                dS.AddImagePath(AppDomain.CurrentDomain.BaseDirectory + @"\images\" + d.name + ".png");
                pageLst.Add(dS);
            }
        }

        private void ResizeDesignerView()
        {
            double newWidth; double newHeight;

            foreach (NewTabPage t in tabLst)
                if (t.tabName == tabControlDesignerView.SelectedTab.Text)
                {
                    //Take the origional height so that it resizes properly every time
                    newWidth = projectDisplaySize.Width * (t.percentageOfZoom / 100);
                    newHeight = projectDisplaySize.Height * (t.percentageOfZoom / 100);

                    t.grpBox.Size = new Size((int)newWidth + 100, (int)newHeight + 100);
                    t.picBox.Size = new Size((int)newWidth, (int)newHeight);
                    t.grpBox.Location = new Point(((tabControlDesignerView.Width / 2) - (t.grpBox.Width / 2)), 50);
                    break;
                }
        }

        //Adds the keyboard shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (projectDisplaySize != new Size(0, 0))
            {
                //ctrl+c creates a new circular button
                if (keyData == (Keys.Control | Keys.C))
                {
                    if (projectDisplaySize != new Size(0, 0))
                    {
                        try
                        {
                            r = Convert.ToInt32(Interaction.InputBox("Please input the radius of the circle", "Radius Of Button"));

                            foreach (DisplayPage d in pageLst)
                                if (d.displayPageName == tabControlDesignerView.SelectedTab.Text)
                                    d.isCreatingCircle = true;
                        }

                        catch (Exception ex)
                        {
                            MessageBox.Show("Error creating button",
                                "Button creation error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1);
                        }
                        return true;
                    }
                }
                //ctrl+b creates a new polygon button
                if (keyData == (Keys.Control | Keys.B))
                {
                    try
                    {
                        if (projectDisplaySize != new Size(0, 0))
                        {
                            isCreatingButton = true;
                            btnFinish.Visible = true;
                            btnCancel.Visible = true;
                            currentButtonType = 0;
                            return true;
                        }
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Error creating button",
                            "Button creation error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1);
                    }
                }
                //ctrl+z cancels the current button being created
                if (keyData == (Keys.Control | Keys.Z))
                {
                    if (projectDisplaySize != new Size(0, 0))
                    {
                        foreach (DisplayPage d in pageLst)
                            if (d.displayPageName == tabControlDesignerView.SelectedTab.Text)
                            {
                                d.isCreatingCircle = false;
                                d.DeleteCurrentButton();
                                d.isCreatingCircle = false;
                            }

                        isCreatingButton = false;
                        btnFinish.Visible = false;
                        btnCancel.Visible = false;
                        return true;
                    }
                }
                //ctrl+n creates a new project
                if (keyData == (Keys.Control | Keys.N))
                {
                    CreateNewProject();
                    return true;
                }
                //ctrl+= creates a new project
                if (keyData == (Keys.Control | Keys.Oemplus))
                {
                    foreach (NewTabPage t in tabLst)
                        if (t.tabName == tabControlDesignerView.SelectedTab.Text)
                            if (t.percentageOfZoom < 500)
                            {
                                t.percentageOfZoom += 10; //Adds 10% to the percentage of zoom
                                ResizeDesignerView();
                                break;
                            }
                    return true;
                }
                //ctrl+- creates a new project
                if (keyData == (Keys.Control | Keys.OemMinus))
                {
                    foreach (NewTabPage t in tabLst)
                        if (t.tabName == tabControlDesignerView.SelectedTab.Text)
                            if (t.percentageOfZoom > 10)
                            {
                                t.percentageOfZoom -= 10; //Subtracts 10% from the percentage of zoom
                                ResizeDesignerView();
                                break;
                            }
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                //If there is an images file used delete all the data inside it
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\images"))
                {
                    DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\images");

                    if (di.GetFiles().Length > 0)
                    {
                        foreach (FileInfo file in di.EnumerateFiles())
                            file.Delete();

                        foreach (DirectoryInfo dir in di.EnumerateDirectories())
                            dir.Delete(true);
                    }
                }
            }
            catch (Exception ex)
            {
                /*MessageBox.Show("Error application was not closed properly :- \n" + ex.StackTrace,
                        "Application shutdown error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);*/
            }
        }

        private void btnRect_Click(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}