using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace XZPToolv3
{
    public partial class BuildForm : Form
    {
        private string sourceDirectory = "";
        private string outputFile = "";

        public BuildForm()
        {
            InitializeComponent();
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select source directory to build from";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    sourceDirectory = dialog.SelectedPath;
                    txtSource.Text = sourceDirectory;
                    UpdateFileCount();
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "XZP Archive|*.xzp|All Files|*.*";
                dialog.Title = "Save XZP Archive";
                dialog.DefaultExt = "xzp";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    outputFile = dialog.FileName;
                    txtOutput.Text = outputFile;
                }
            }
        }

        private void UpdateFileCount()
        {
            if (string.IsNullOrEmpty(sourceDirectory) || !Directory.Exists(sourceDirectory))
            {
                lblFileCount.Text = "0 files";
                return;
            }

            try
            {
                int count = CountFiles(sourceDirectory);
                lblFileCount.Text = $"{count} file(s) will be included";
            }
            catch
            {
                lblFileCount.Text = "Error counting files";
            }
        }

        private int CountFiles(string directory)
        {
            int count = Directory.GetFiles(directory).Length;

            foreach (string subDir in Directory.GetDirectories(directory))
            {
                count += CountFiles(subDir);
            }

            return count;
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(sourceDirectory) || !Directory.Exists(sourceDirectory))
            {
                MessageBox.Show("Please select a valid source directory.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                MessageBox.Show("Please select an output file.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int version = rbVersion3.Checked ? 3 : 1;

            using (Form progressForm = new Form())
            {
                progressForm.Text = "Building XZP...";
                progressForm.Size = new Size(400, 100);
                progressForm.StartPosition = FormStartPosition.CenterParent;
                progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                progressForm.MaximizeBox = false;
                progressForm.MinimizeBox = false;

                Label label = new Label();
                label.Text = "Building XZP archive, please wait...";
                label.AutoSize = true;
                label.Location = new Point(20, 30);
                progressForm.Controls.Add(label);

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (s, args) =>
                {
                    Xuiz.BuildXuiz(sourceDirectory, outputFile, version);
                };
                worker.RunWorkerCompleted += (s, args) =>
                {
                    progressForm.Close();

                    if (args.Error != null)
                    {
                        MessageBox.Show("Error building XZP: " + args.Error.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("XZP archive built successfully!", "Success",
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
