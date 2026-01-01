using System;
using System.Drawing;
using System.Windows.Forms;

namespace XZPToolv3
{
    public partial class OverwriteDialog : Form
    {
        public bool AlwaysOverwrite { get; private set; }

        public OverwriteDialog(string fileName)
        {
            InitializeComponent();
            lblMessage.Text = $"File '{fileName}' already exists in archive.\n\nDo you want to overwrite it?";
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            AlwaysOverwrite = chkAlwaysOverwrite.Checked;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
