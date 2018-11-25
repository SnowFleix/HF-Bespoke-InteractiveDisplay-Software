/*  AUTHOR          :   FELIX ENGLAND
 *  VERSION         :   V1.0 5/10/2018
 *  
 *  DisplayPage.cs
 *  PURPOSE         :   Class for creating pages
 *  LAST MODIFIED   :   11/10/2018
 *  MODIFIED BY     :   FELIX ENGLAND
 *  NOTES           :   
 *  
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace HFBespoke
{
    class DisplayPage
    {
        public string displayPageName;
        public bool isCreatingCircle = false;
        Image backgroundImage;
        string imgPath;
        List<Point> currentButton;
        List<CustomButton> buttonList;
        List<CircleButton> circleBtnLst;

        public DisplayPage(string passedName)
        {
            displayPageName = passedName;
            currentButton = new List<Point>();
            buttonList = new List<CustomButton>();
            circleBtnLst = new List<CircleButton>();
        }

        public void AddImagePath(string passedPath)
        {
            imgPath = passedPath;
            backgroundImage = Image.FromFile(passedPath);
        }

        public void AddBackgroundImage(Image passedBI)
        {
            backgroundImage = passedBI;
        }

        public void AddButton()
        {
            DeleteCurrentButton();
        }

        public void AddCompletedButton(CustomButton passedButton)
        {
            DeleteCurrentButton();
            buttonList.Add(passedButton);
        }

        public void DeleteCustomButton(Point[] polyArr)
        {
            //Assimes that two buttons won't have the same ponts array as another
            foreach (CustomButton c in buttonList)
                if (polyArr == c.GetPoly())
                {
                    buttonList.Remove(c);
                    break;//Because the list was modified it'll crash if I try to itterate through it again
                }
        }

        public void DeleteCircleButton(Point centre, int radius)
        {
            //Assumes that two buttons won't have the same centre and radius
            foreach (CircleButton c in circleBtnLst)
                if (c.GetCentre() == centre && c.GetRadius() == radius)
                {
                    circleBtnLst.Remove(c);
                    break;//Because the list was modified it'll crash if I try to itterate through it again
                }
        }

        public void AlterButton()
        {

        }

        public void AddCircleButton(Point c, int r, string pageLink)
        {
            CircleButton newBtn = new CircleButton(c, r);
            newBtn.SetPageLink(pageLink);
            circleBtnLst.Add(newBtn);
        }

        public void AddPointToCurrentButton(Point p)
        {
            currentButton.Add(p);
        }

        public void FinishButton(int btnType, string linkedPage)
        {
            CustomButton newBtn = new CustomButton(currentButton.ToArray(), btnType);
            newBtn.SetPageLink(linkedPage);
            buttonList.Add(newBtn);
            currentButton.Clear();
        }

        public void DeleteCurrentButton()
        {
            currentButton.Clear();
        }

        public string GetImagePath()
        {
            return imgPath;
        }

        public string GetByteArrayFromBachgroundImage()
        {
            using (Image image = Image.FromFile(imgPath))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    m.Close();
                    return base64String;
                }
            }
        }

        public IReadOnlyList<CustomButton> GetButtonList()
        {
            return buttonList;
        }

        public IReadOnlyList<Point> GetCurrentButton()
        {
            return currentButton;
        }

        public IReadOnlyList<CircleButton> GetCircleBtnLst()
        {
            return circleBtnLst;
        }

        public Image GetBackgroundImage()
        {
            return Image.FromFile(imgPath);
        }
    }
}
