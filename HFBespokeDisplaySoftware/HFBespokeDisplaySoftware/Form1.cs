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

namespace HFBespokeDisplaySoftware
{
    public partial class Form1 : Form
    {
        private Size projectDisplaySize;
        private List<DisplayPage> pageLst;
        private bool isRunning = false;
        private DisplayPage currentDisplay;

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

        public Form1()
        {
            InitializeComponent();
            pageLst = new List<DisplayPage>();
            lblSelectedFolder.Visible = false;

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

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            XMLConverter xml = new XMLConverter();

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
                {
                    d.SaveImage();
                    if (d.displayPageName == "Main Page")
                        main = d.GetBackgroundImage();
                }

                if (main != null)
                {
                    int screenWidth = main.Width; int screenHeight = main.Height;
                    projectDisplaySize = new Size(screenWidth, screenHeight);

                    lblSelectedFolder.Visible = true;
                    lblSelectedFolder.Text = "CHOSEN FOLDER : " + filePath;
                }
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

        private void btnRun_Click(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.None;
            picBoxMain.Size = projectDisplaySize;
            picBoxMain.Location = new Point(0, 0);
            this.Location = new Point(0, 0);
            this.Size = projectDisplaySize;
            DisplayNewPage("Main Page");
            isRunning = true;
            btnSelectFolder.Visible = false;
            btnRun.Visible = false;
            lblSelectedFolder.Visible = false;
            GC.Collect(); //Deletes all the old image files from memory
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

                dS.AddBackgroundImage(xml.GetImageFromBase64(d.backgroundImage));
                pageLst.Add(dS);
            }
        }

        private void DisplayNewPage(string pageName)
        {
            foreach (DisplayPage d in pageLst)
                if (d.displayPageName == pageName)
                {
                    picBoxMain.BackgroundImage = d.GetBackgroundImage();
                    currentDisplay = d;
                    break;
                }
        }

        private void picBoxMain_Click(object sender, EventArgs e)
        {
            if (isRunning == true)
            {
                DetectIfPointIsInside ifPointIsInside = new DetectIfPointIsInside(projectDisplaySize.Width);
                //Gets the position of the mouse cursor inside the picturebox
                MouseEventArgs me = (MouseEventArgs)e;
                Point p = me.Location;

                foreach (CustomButton c in currentDisplay.GetButtonList())
                    if (ifPointIsInside.DoesIntersect(c.GetPoly(), c.GetPoly().Length, p) == true)
                        DisplayNewPage(c.GetPageLink());

                foreach (CircleButton c in currentDisplay.GetCircleBtnLst())
                    if (ifPointIsInside.DoesIntersectCircle(c.GetRadius(), p, c.GetCentre()))
                        DisplayNewPage(c.GetPageLink());
            }
        }
    }
}