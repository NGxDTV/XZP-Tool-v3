using System;
using System.Windows.Forms;

namespace XZPToolv3
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            lblTitle.Text = "XZP Tool v3.0";
            lblSubtitle.Text = "Advanced XZP Archive Manager";

            txtCredits.Text =
                "Created by: NGxD TV\r\n\r\n" +
                "Credits:\r\n" +
                "- XZP Tool v2\r\n" +
                "- XuiWorkshop sources";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
