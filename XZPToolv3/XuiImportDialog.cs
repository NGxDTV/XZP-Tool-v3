using System;
using System.Windows.Forms;

namespace XZPToolv3
{
    public partial class XuiImportDialog : Form
    {
        public XuiImportMode SelectedMode { get; private set; }
        public bool RememberChoice { get; private set; }
        private readonly bool isXurSource;

        public XuiImportDialog(bool isXur)
        {
            InitializeComponent();
            SelectedMode = XuiImportMode.ImportOriginal;
            isXurSource = isXur;
        }

        private void XuiImportDialog_Load(object sender, EventArgs e)
        {
            if (isXurSource)
            {
                Text = "XUR Import Options";
                label1.Text = "You are importing XUR file(s). How would you like to handle the XUR import?";
                radioXuiOnly.Text = "Import XUR only (no conversion)";
                radioConvertToXur.Text = "Convert to XUI and import only XUI (discard XUR)";
                radioBoth.Text = "Import both XUR and XUI (XUR will be converted and both added)";
            }
            else
            {
                Text = "XUI Import Options";
                label1.Text = "You are importing XUI file(s). How would you like to handle the XUI import?";
                radioXuiOnly.Text = "Import XUI only (no conversion)";
                radioConvertToXur.Text = "Convert to XUR and import only XUR (discard XUI)";
                radioBoth.Text = "Import both XUI and XUR (XUI will be converted and both added)";
            }

            radioXuiOnly.Checked = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Determine selected mode
            if (radioXuiOnly.Checked)
                SelectedMode = XuiImportMode.ImportOriginal;
            else if (radioConvertToXur.Checked)
                SelectedMode = XuiImportMode.ConvertOnly;
            else if (radioBoth.Checked)
                SelectedMode = XuiImportMode.Both;

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
