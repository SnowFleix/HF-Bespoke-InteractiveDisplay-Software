using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HFBespokeDisplaySoftware
{
    class DisplayPage
    {
        public string displayPageName;
        Image backgroundImage;
        string imagePath;
        List<CustomButton> buttonList;
        List<CircleButton> circleBtnLst;

        public DisplayPage(string passedName)
        {
            displayPageName = passedName;
            buttonList = new List<CustomButton>();
            circleBtnLst = new List<CircleButton>();
        }

        public void SaveImage()
        {
            backgroundImage.Save(AppDomain.CurrentDomain.BaseDirectory + @"\images\" + displayPageName + ".png");
            imagePath = AppDomain.CurrentDomain.BaseDirectory + @"\images\" + displayPageName + ".png";
            backgroundImage = null;
        }

        public void AddBackgroundImage(Image passedBI)
        {
            backgroundImage = passedBI;
        }

        public void AddCompletedButton(CustomButton passedButton)
        {
            buttonList.Add(passedButton);
        }

        public void AddCircleButton(Point c, int r, string pageLink)
        {
            CircleButton newBtn = new CircleButton(c, r);
            newBtn.SetPageLink(pageLink);
            circleBtnLst.Add(newBtn);
        }

        public void SetImagePath(string path)
        {
            imagePath = path;
        }

        public IReadOnlyList<CustomButton> GetButtonList()
        {
            return buttonList;
        }

        public IReadOnlyList<CircleButton> GetCircleBtnLst()
        {
            return circleBtnLst;
        }

        public Image GetBackgroundImage()
        {
            return Image.FromFile(imagePath);
        }
    }
}
