/*  AUTHOR          :   FELIX ENGLAND
 *  VERSION         :   V1.0 5/10/2018
 *  
 *  SetLinkPage.cs
 *  PURPOSE         :   Main code for the form used for the user to select a page link
 *  LAST MODIFIED   :   11/10/2018
 *  MODIFIED BY     :   FELIX ENGLAND
 *  NOTES           :   
 *  
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFBespoke
{
    public partial class SelectLinkPage : Form
    {
        public string selectedPage { get; set; }
        private List<string> pageList;

        public SelectLinkPage(List<string> passedPageList)
        {
            InitializeComponent();
            pageList = passedPageList;
        }

        private void SelectLinkPage_Load(object sender, EventArgs e)
        {
            lstBoxPages.Items.AddRange(pageList.ToArray());
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            this.selectedPage = lstBoxPages.SelectedItem.ToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
