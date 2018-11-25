/*  AUTHOR          :   FELIX ENGLAND
 *  VERSION         :   V1.0 5/10/2018
 *  
 *  Circle.cs
 *  PURPOSE         :   Class for the circular buttons
 *  LAST MODIFIED   :   11/10/2018
 *  MODIFIED BY     :   FELIX ENGLAND
 *  NOTES           :   
 *  
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HFBespoke
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