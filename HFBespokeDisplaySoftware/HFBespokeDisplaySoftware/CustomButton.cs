using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HFBespokeDisplaySoftware
{
    class CustomButton
    {
        //0=Polygon
        //1=Circle
        //2=Elipses
        /*OBSOLITE*/
        private int btnType;
        private string pageLink;
        private Point[] poly;

        public CustomButton(Point[] passedPoly, int passedType)
        {
            poly = passedPoly;
            btnType = passedType;
        }

        public void SetPageLink(string passedLink)
        {
            pageLink = passedLink;
        }

        public string GetPageLink()
        {
            return pageLink;
        }

        public int GetButtonType()
        {
            return btnType;
        }

        public Point[] GetPoly()
        {
            return poly;
        }
    }
}
