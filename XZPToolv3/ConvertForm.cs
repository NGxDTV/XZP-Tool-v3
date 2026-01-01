using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace XZPToolv3
{
    public partial class ConvertForm : Form
    {
        private string xzpFile;
        private int currentVersion;

        public ConvertForm(string filePath, int version)
        {
            InitializeComponent();
            xzpFile = filePath;
            currentVersion = version;
        }

        private void ConvertForm_Load(object sender, EventArgs e)
        {
            lblCurrentVersion.Text = $"Current Version: {currentVersion}";

            if (currentVersion == 1)
            {
                rbVersion3.Checked = true;
                rbVersion1.Enabled = false;
            }
            else
            {
                rbVersion1.Checked = true;
                rbVersion3.Enabled = false;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            int targetVersion = rbVersion1.Checked ? 1 : 3;

            if (targetVersion == currentVersion)
            {
                MessageBox.Show("Target version is the same as current version.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirmResult = MessageBox.Show(
                $"Convert XZP archive from Version {currentVersion} to Version {targetVersion}?\n\n" +
                "This will modify the archive file directly.",
                "Confirm Conversion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            using (Form progressForm = new Form())
            {
                progressForm.Text = "Converting...";
                progressForm.Size = new Size(400, 100);
                progressForm.StartPosition = FormStartPosition.CenterParent;
                progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                progressForm.MaximizeBox = false;
                progressForm.MinimizeBox = false;

                Label label = new Label();
                label.Text = $"Converting XZP to Version {targetVersion}...";
                label.AutoSize = true;
                label.Location = new Point(20, 30);
                progressForm.Controls.Add(label);

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (s, args) =>
                {
                    Xuiz.ConvertXzp(xzpFile, targetVersion);
                };
                worker.RunWorkerCompleted += (s, args) =>
                {
                    progressForm.Close();

                    if (args.Error != null)
                    {
                        MessageBox.Show("Error converting archive: " + args.Error.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Archive successfully converted to Version {targetVersion}!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                };

                worker.RunWorkerAsync();
                progressForm.ShowDialog(this);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
