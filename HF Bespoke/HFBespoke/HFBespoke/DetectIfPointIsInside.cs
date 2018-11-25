/*  AUTHOR          :   FELIX ENGLAND
 *  VERSION         :   V1.0 5/10/2018
 *  
 *  DetectIfPointIsInside.cs
 *  PURPOSE         :   Code for detecting whether a point lies within an arbitrary polygon/shape or circle
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
using System.Windows.Forms;

namespace HFBespoke
{
    class DetectIfPointIsInside
    {
        int maxXPoint;

        //Might only need the display size
        public DetectIfPointIsInside(int displaySizeX)
        {
            maxXPoint = displaySizeX;
        }

        public bool OnSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;
            return false;
        }

        // To find orientation of ordered triplet (p, q, r). 
        // The function returns following values 
        // 0 --> p, q and r are colinear 
        // 1 --> Clockwise 
        // 2 --> Counterclockwise 
        public int orientation(Point p, Point q, Point r)
        {
            int val = (q.Y - p.Y) * (r.X - q.X) -
                  (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0;  // colinear 
            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }

        // The function that returns true if line segment 'p1q1' 
        // and 'p2q2' intersect. 
        public bool doIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

            // p1, q1 and p2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }

        public bool DoesIntersect(Point[] polygon, int n, Point p)
        {
            // There must be at least 3 vertices in polygon[] 
            if (n < 3) return false;

            // Create a point for line segment from p to infinite 
            Point extreme = new Point(maxXPoint, p.Y);

            // Count intersections of the above line with sides of polygon 
            int count = 0, i = 0;
            do
            {
                int next = (i + 1) % n;

                // Check if the line segment from 'p' to 'extreme' intersects 
                // with the line segment from 'polygon[i]' to 'polygon[next]' 
                if (doIntersect(polygon[i], polygon[next], p, extreme))
                {
                    // If the point 'p' is colinear with line segment 'i-next', 
                    // then check if it lies on segment. If it lies, return true, 
                    // otherwise false 
                    if (orientation(polygon[i], p, polygon[next]) == 0)
                        return OnSegment(polygon[i], p, polygon[next]);

                    count++;
                }
                i = next;
            } while (i != 0);

            //If it crosses the boarder an odd amount of times it's inside the polygon
            if (count % 2 == 1)
                return true;
            return false;
        }

        //Detects if the point p is inside a circle with a centre point of c and a radius or r
        public bool DoesIntersectCircle(int r, Point p, Point c)
        {
            int d = (p.X - c.X) * (p.X - c.X) + (p.Y - c.Y) * (p.Y - c.Y);
            r *= r;

            if (d <= r)
                return true;
            else
                return false;
        }
    }
}
