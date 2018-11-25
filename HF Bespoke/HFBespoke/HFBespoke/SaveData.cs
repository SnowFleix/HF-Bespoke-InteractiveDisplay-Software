using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HFBespoke
{
    //Useless class atm
    class SaveData
    {
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
            public Image backgroundImage { get; set; }
            public List<CustomButtonSerialise> buttonList { get; set; }
            public List<CircleButtonSerialise> circleBtnLst { get; set; }
        }

        public string FormatToXML(List<DisplayPage> dLst)
        {
            List<DisplayPageSerialise> displayLstS = new List<DisplayPageSerialise>();
            XMLConverter xml = new XMLConverter();

            foreach(DisplayPage d in dLst)
            {
                DisplayPageSerialise dS = new DisplayPageSerialise();
                dS.name = d.displayPageName;
                dS.backgroundImage = d.GetBackgroundImage();
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
                    }
                }

                displayLstS.Add(dS);
            }

            return xml.ToXml(displayLstS);
        }
    }
}
