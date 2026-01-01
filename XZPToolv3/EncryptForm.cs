using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace XZPToolv3
{
    public partial class EncryptForm : Form
    {
        private string xzpFilePath;
        private byte[] encryptedData;

        public EncryptForm(string xzpPath)
        {
            InitializeComponent();
            xzpFilePath = xzpPath;
        }

        private void EncryptForm_Load(object sender, EventArgs e)
        {
            // Load templates into combo box
            cmbTemplate.Items.Clear();
            foreach (OutputTemplate template in AppSettings.Instance.Templates)
            {
                cmbTemplate.Items.Add(template.Name);
            }

            if (cmbTemplate.Items.Count > 0)
            {
                int index = AppSettings.Instance.SelectedTemplateIndex;
                if (index >= 0 && index < cmbTemplate.Items.Count)
                    cmbTemplate.SelectedIndex = index;
                else
                    cmbTemplate.SelectedIndex = 0;
            }

            // Perform encryption
            EncryptXzp();
        }

        private void EncryptXzp()
        {
            try
            {
                // Read XZP file
                byte[] plainData = File.ReadAllBytes(xzpFilePath);

                // Get master key
                byte[] masterKey = AppSettings.Instance.GetMasterKeyBytes();

                // Encrypt using RC4
                encryptedData = RC4.Process(masterKey, plainData);

                // Generate output
                GenerateOutput();

                lblStatus.Text = $"Encrypted {encryptedData.Length} bytes successfully.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Encryption failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
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

        private void GenerateOutput()
        {
            if (encryptedData == null || cmbTemplate.SelectedIndex < 0)
                return;

            OutputTemplate template = AppSettings.Instance.Templates[cmbTemplate.SelectedIndex];

            StringBuilder sb = new StringBuilder();

            // Add header (process escape sequences and replace size placeholder)
            string header = ProcessEscapeSequences(template.Header);
            header = header.Replace("{SIZE}", encryptedData.Length.ToString());
            sb.Append(header);

            // Add encrypted bytes
            for (int i = 0; i < encryptedData.Length; i += template.BytesPerLine)
            {
                sb.Append(ProcessEscapeSequences(template.LinePrefix));

                int bytesInLine = Math.Min(template.BytesPerLine, encryptedData.Length - i);
                for (int j = 0; j < bytesInLine; j++)
                {
                    int byteIndex = i + j;
                    sb.Append(string.Format(template.ByteFormat, encryptedData[byteIndex]));

                    if (j < bytesInLine - 1)
                    {
                        sb.Append(ProcessEscapeSequences(template.ByteSeparator));
                    }
                }

                // Add line suffix only if not the last line
                if (i + template.BytesPerLine < encryptedData.Length)
                {
                    sb.Append(ProcessEscapeSequences(template.LineSuffix));
                }

                sb.Append("\n");
            }

            // Add footer
            sb.Append(ProcessEscapeSequences(template.Footer));

            txtOutput.Text = sb.ToString();
        }

        private void cmbTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerateOutput();

            // Save selected template index
            AppSettings.Instance.SelectedTemplateIndex = cmbTemplate.SelectedIndex;
            AppSettings.Instance.Save();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                OutputTemplate template = AppSettings.Instance.Templates[cmbTemplate.SelectedIndex];

                // Determine file extension based on template name
                if (template.Name.Contains("C++"))
                {
                    dialog.Filter = "C++ Source File|*.cpp|Header File|*.h|All Files|*.*";
                    dialog.DefaultExt = "cpp";
                }
                else if (template.Name.Contains("C#"))
                {
                    dialog.Filter = "C# Source File|*.cs|All Files|*.*";
                    dialog.DefaultExt = "cs";
                }
                else if (template.Name.Contains("Python"))
                {
                    dialog.Filter = "Python File|*.py|All Files|*.*";
                    dialog.DefaultExt = "py";
                }
                else
                {
                    dialog.Filter = "Text File|*.txt|All Files|*.*";
                    dialog.DefaultExt = "txt";
                }

                dialog.FileName = Path.GetFileNameWithoutExtension(xzpFilePath) + "_encrypted";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(dialog.FileName, txtOutput.Text);
                        MessageBox.Show("File saved successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save file: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(txtOutput.Text);
                MessageBox.Show("Copied to clipboard!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
