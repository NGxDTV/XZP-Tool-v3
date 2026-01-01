using System;
using System.Windows.Forms;

namespace XZPToolv3
{
    /// <summary>
    /// Dialog for choosing how to open XUI/XUR files
    /// </summary>
    public partial class XuiOpenDialog : Form
    {
        public enum OpenMode
        {
            ExternalProgram,
            InternalViewer,
            AddToArchive
        }

        public OpenMode SelectedMode { get; private set; }
        public bool RememberChoice { get; private set; }
        private bool isXurFile;

        public XuiOpenDialog(string fileName, bool isXur)
        {
            InitializeComponent();
            isXurFile = isXur;
            lblFileName.Text = $"File: {fileName}";
        }

        private void XuiOpenDialog_Load(object sender, EventArgs e)
        {
            if (isXurFile)
            {
                radioExternal.Text = "Convert to XUI and open in external program";
                radioInternal.Text = "Convert to XUI and view in XZP Tool";
            }
            else
            {
                radioExternal.Text = "Open in external program";
                radioInternal.Text = "View in XZP Tool (XUI/XUR viewer)";
            }

            radioInternal.Checked = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (radioExternal.Checked)
                SelectedMode = OpenMode.ExternalProgram;
            else if (radioInternal.Checked)
                SelectedMode = OpenMode.InternalViewer;
            else if (radioArchive.Checked)
                SelectedMode = OpenMode.AddToArchive;

            RememberChoice = chkRemember.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
