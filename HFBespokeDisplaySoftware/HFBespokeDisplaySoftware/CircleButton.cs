using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HFBespokeDisplaySoftware
{
    class CircleButton
    {
        private string pageLink;
        private int radius;
        private Point centre;

        public CircleButton(Point passedCentre, int passedRadius)
        {
            radius = passedRadius;
            centre = passedCentre;
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
            return 1;
        }

        public Point GetCentre()
        {
            return centre;
        }

        public int GetRadius()
        {
            return radius;
        }
    }
}
