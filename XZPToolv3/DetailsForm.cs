using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace XZPToolv3
{
    public partial class DetailsForm : Form
    {
        private string xzpFile;

        public DetailsForm(string filePath)
        {
            InitializeComponent();
            xzpFile = filePath;
        }

        private void DetailsForm_Load(object sender, EventArgs e)
        {
            try
            {
                XzpMetadata metadata = Xuiz.GetMetadata(xzpFile);
                FileInfo fileInfo = new FileInfo(xzpFile);

                txtFileName.Text = Path.GetFileName(xzpFile);
                txtFilePath.Text = Path.GetDirectoryName(xzpFile);
                txtFileSize.Text = FormatFileSize(metadata.FileSize);
                txtMagic.Text = metadata.GetMagicString() + $" (0x{metadata.Magic:X8})";
                txtVersion.Text = metadata.Version.ToString();
                txtDataOffset.Text = $"0x{metadata.DataOffset:X} ({metadata.DataOffset} bytes)";
                txtEntryCount.Text = metadata.EntryCount.ToString();
                txtModified.Text = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                txtCreated.Text = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");

                // Calculate compression info
                long totalUncompressedSize = 0;
                var entries = Xuiz.GetEntries(xzpFile);
                foreach (var entry in entries)
                {
                    totalUncompressedSize += entry.Size;
                }

                txtTotalDataSize.Text = FormatFileSize(totalUncompressedSize);

                if (totalUncompressedSize > 0)
                {
                    double ratio = (double)metadata.FileSize / totalUncompressedSize * 100;
                    txtCompressionRatio.Text = $"{ratio:0.00}%";
                }
                else
                {
                    txtCompressionRatio.Text = "N/A";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading archive details: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
