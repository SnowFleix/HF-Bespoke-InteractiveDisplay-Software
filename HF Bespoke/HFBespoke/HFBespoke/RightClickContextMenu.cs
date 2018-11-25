using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace HFBespoke
{
    class RightClickContextMenu : ContextMenu
    {
        public Point p = new Point(0, 0);

        //Don't know what this used to do but now it sets the coordinates of the mouse when the context menu was opened
        protected override void OnPopup(EventArgs e)
        {
            double xCoordinate = Cursor.Position.X;
            double yCoordinate = Cursor.Position.Y;
            p.X = (int)xCoordinate; p.Y = (int)yCoordinate;
        }
    }
}
