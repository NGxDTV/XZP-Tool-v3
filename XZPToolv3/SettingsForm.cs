using System;
using System.Windows.Forms;

namespace XZPToolv3
{
    public partial class SettingsForm : Form
    {
        private AppSettings settings;
        private int selectedTemplateIndex = -1;

        public SettingsForm()
        {
            InitializeComponent();
            settings = AppSettings.Instance;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            // Load general settings
            chkAlwaysOverwrite.Checked = settings.AlwaysOverwrite;
            txtMasterKey.Text = settings.MasterKey;
            RefreshXuiExtensionList();

            // Load templates
            RefreshTemplateList();
        }

        private void RefreshXuiExtensionList()
        {
            lstXuiExtensions.Items.Clear();
            foreach (string path in settings.XuiExtensionFiles)
            {
                if (!string.IsNullOrWhiteSpace(path))
                    lstXuiExtensions.Items.Add(path);
            }
        }

        private void RefreshTemplateList()
        {
            lstTemplates.Items.Clear();
            foreach (OutputTemplate template in settings.Templates)
            {
                lstTemplates.Items.Add(template.Name);
            }

            if (lstTemplates.Items.Count > 0 && selectedTemplateIndex < 0)
            {
                selectedTemplateIndex = 0;
                lstTemplates.SelectedIndex = 0;
            }
            else if (selectedTemplateIndex >= 0 && selectedTemplateIndex < lstTemplates.Items.Count)
            {
                lstTemplates.SelectedIndex = selectedTemplateIndex;
            }
        }

        private void lstTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstTemplates.SelectedIndex < 0)
            {
                ClearTemplateFields();
                return;
            }

            selectedTemplateIndex = lstTemplates.SelectedIndex;
            OutputTemplate template = settings.Templates[selectedTemplateIndex];

            txtTemplateName.Text = template.Name;
            txtHeader.Text = template.Header;
            txtLinePrefix.Text = template.LinePrefix;
            numBytesPerLine.Value = template.BytesPerLine;
            txtByteFormat.Text = template.ByteFormat;
            txtByteSeparator.Text = template.ByteSeparator;
            txtLineSuffix.Text = template.LineSuffix;
            txtFooter.Text = template.Footer;

            btnDeleteTemplate.Enabled = true;
        }

        private void ClearTemplateFields()
        {
            txtTemplateName.Text = "";
            txtHeader.Text = "";
            txtLinePrefix.Text = "";
            numBytesPerLine.Value = 12;
            txtByteFormat.Text = "";
            txtByteSeparator.Text = "";
            txtLineSuffix.Text = "";
            txtFooter.Text = "";

            btnDeleteTemplate.Enabled = false;
        }

        private void btnNewTemplate_Click(object sender, EventArgs e)
        {
            OutputTemplate newTemplate = new OutputTemplate
            {
                Name = "New Template",
                Header = "// New Template\n",
                LinePrefix = "\t",
                BytesPerLine = 16,
                ByteFormat = "0x{0:X2}",
                ByteSeparator = ", ",
                LineSuffix = ",",
                Footer = "\n"
            };

            settings.Templates.Add(newTemplate);
            selectedTemplateIndex = settings.Templates.Count - 1;
            RefreshTemplateList();
        }

        private void btnSaveTemplate_Click(object sender, EventArgs e)
        {
            if (selectedTemplateIndex < 0)
                return;

            OutputTemplate template = settings.Templates[selectedTemplateIndex];

            template.Name = txtTemplateName.Text;
            template.Header = txtHeader.Text;
            template.LinePrefix = txtLinePrefix.Text;
            template.BytesPerLine = (int)numBytesPerLine.Value;
            template.ByteFormat = txtByteFormat.Text;
            template.ByteSeparator = txtByteSeparator.Text;
            template.LineSuffix = txtLineSuffix.Text;
            template.Footer = txtFooter.Text;

            RefreshTemplateList();

            MessageBox.Show("Template saved!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDeleteTemplate_Click(object sender, EventArgs e)
        {
            if (selectedTemplateIndex < 0)
                return;

            if (settings.Templates.Count <= 1)
            {
                MessageBox.Show("Cannot delete the last template!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this template?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                settings.Templates.RemoveAt(selectedTemplateIndex);
                selectedTemplateIndex = -1;
                RefreshTemplateList();
                ClearTemplateFields();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Save general settings
            settings.AlwaysOverwrite = chkAlwaysOverwrite.Checked;
            settings.MasterKey = txtMasterKey.Text;
            settings.XuiExtensionFiles = new System.Collections.Generic.List<string>();
            foreach (var item in lstXuiExtensions.Items)
                settings.XuiExtensionFiles.Add(item.ToString());

            // Save to file
            settings.Save();

            MessageBox.Show("Settings saved successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Reload settings to discard changes
            AppSettings.Reload();

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnResetKey_Click(object sender, EventArgs e)
        {
            txtMasterKey.Text = "9B BC 90 A6 AE 90 A1 3D AC 7F 36 C6 E8 0A 8C 02";
        }

        private void btnAddXuiExtension_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "XML Files|*.xml|All Files|*.*";
                dialog.Title = "Add XUI Extension XML";
                dialog.Multiselect = true;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (string path in dialog.FileNames)
                    {
                        if (!lstXuiExtensions.Items.Contains(path))
                            lstXuiExtensions.Items.Add(path);
                    }
                }
            }
        }

        private void btnRemoveXuiExtension_Click(object sender, EventArgs e)
        {
            if (lstXuiExtensions.SelectedItems.Count == 0)
                return;

            while (lstXuiExtensions.SelectedItems.Count > 0)
            {
                lstXuiExtensions.Items.Remove(lstXuiExtensions.SelectedItems[0]);
            }
        }

        private string ProcessEscapeSequences(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Replace escape sequences
            text = text.Replace("\\n", "\n");
            text = text.Replace("\\t", "\t");
            text = text.Replace("\\r", "\r");
            text = text.Replace("\\\\", "\\");
            text = text.Replace("{{", "{");
            text = text.Replace("}}", "}");

            return text;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (selectedTemplateIndex < 0)
                return;

            // Create a temporary template with current values
            OutputTemplate previewTemplate = new OutputTemplate
            {
                Name = txtTemplateName.Text,
                Header = txtHeader.Text,
                LinePrefix = txtLinePrefix.Text,
                BytesPerLine = (int)numBytesPerLine.Value,
                ByteFormat = txtByteFormat.Text,
                ByteSeparator = txtByteSeparator.Text,
                LineSuffix = txtLineSuffix.Text,
                Footer = txtFooter.Text
            };

            // Generate preview with sample data
            byte[] sampleData = new byte[] { 0x9B, 0xBC, 0x90, 0xA6, 0xAE, 0x90, 0xA1, 0x3D, 0xAC, 0x7F, 0x36, 0xC6, 0xE8, 0x0A, 0x8C, 0x02 };

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // Process escape sequences in header and replace size
            string header = ProcessEscapeSequences(previewTemplate.Header);
            sb.Append(header.Replace("{SIZE}", sampleData.Length.ToString()));

            for (int i = 0; i < sampleData.Length; i += previewTemplate.BytesPerLine)
            {
                sb.Append(ProcessEscapeSequences(previewTemplate.LinePrefix));

                int bytesInLine = Math.Min(previewTemplate.BytesPerLine, sampleData.Length - i);
                for (int j = 0; j < bytesInLine; j++)
                {
                    int byteIndex = i + j;
                    sb.Append(string.Format(previewTemplate.ByteFormat, sampleData[byteIndex]));

                    if (j < bytesInLine - 1)
                    {
                        sb.Append(ProcessEscapeSequences(previewTemplate.ByteSeparator));
                    }
                }

                if (i + previewTemplate.BytesPerLine < sampleData.Length)
                {
                    sb.Append(ProcessEscapeSequences(previewTemplate.LineSuffix));
                }

                sb.Append("\n");
            }

            sb.Append(ProcessEscapeSequences(previewTemplate.Footer));

            MessageBox.Show(sb.ToString(), "Template Preview",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
