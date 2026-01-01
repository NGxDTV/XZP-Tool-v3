using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace XZPToolv3
{
    public partial class MainForm : Form
    {
        private XuizParams xuizParams;
        private int currentMode = 0; // 0 = filesystem, 1 = inside XZP
        private string currentPath = "";
        private string currentXzpFile = "";
        private ListViewItem editingItem = null;
        private int currentXzpVersion = 3;
        private int currentXzpDataOffset = 0;
        private int currentXzpEntryCount = 0;
        private long currentXzpFileSize = 0;

        // Overwrite settings
        private bool alwaysOverwrite = false;

        // XUI/XUR session preferences
        private XuiOpenDialog.OpenMode? xuiOpenMode = null;
        private XuiImportMode? xuiImportMode = null;
        private XuiImportMode? xurImportMode = null;

        // Sorting
        private int sortColumn = -1;
        private SortOrder sortOrder = SortOrder.None;

        // All items cache for search
        private List<ListViewItem> allItems = new List<ListViewItem>();

        public MainForm()
        {
            InitializeComponent();
            xuizParams = new XuizParams();
            InitializeView();
        }

        private void InitializeView()
        {
            // Setup ListView
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.AllowDrop = true;
            listView.LabelEdit = true;
            listView.MultiSelect = true;

            // Add columns
            listView.Columns.Add("Name", 250);
            listView.Columns.Add("Type", 100);
            listView.Columns.Add("Size", 100);
            listView.Columns.Add("Offset", 80);
            listView.Columns.Add("Modified", 150);

            // Load system icons into ImageList
            SystemIconHelper.PopulateImageList(imageList);

            // Enable drag & drop
            listView.DragEnter += ListView_DragEnter;
            listView.DragDrop += ListView_DragDrop;
            listView.ItemDrag += ListView_ItemDrag;

            // Column click for sorting
            listView.ColumnClick += ListView_ColumnClick;

            // Set initial path
            currentPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            RefreshFileView();
            UpdateMenuState();
        }

        // ===== Drag & Drop Handlers =====

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            if (currentMode == 1 && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void ListView_DragDrop(object sender, DragEventArgs e)
        {
            if (currentMode != 1) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                AddFilesToArchive(files);
            }
        }

        private void ListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
                return;

            List<string> exportPaths = new List<string>();
            string tempRoot = null;

            try
            {
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    if (item.Tag?.ToString() == "UP_DIR")
                        continue;

                    if (currentMode == 1)
                    {
                        if (tempRoot == null)
                        {
                            tempRoot = Path.Combine(Path.GetTempPath(), "XZPTool", "DragOut", Guid.NewGuid().ToString("N"));
                            Directory.CreateDirectory(tempRoot);
                        }

                        XuizEntry entry = (XuizEntry)item.Tag;
                        string tempPath = Path.Combine(tempRoot, entry.Name);
                        Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
                        Xuiz.ExtractEntry(currentXzpFile, entry, tempPath);
                        exportPaths.Add(tempPath);
                    }
                    else
                    {
                        string path = item.Tag?.ToString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                            exportPaths.Add(path);
                    }
                }

                if (exportPaths.Count == 0)
                    return;

                DataObject data = new DataObject(DataFormats.FileDrop, exportPaths.ToArray());
                listView.DoDragDrop(data, DragDropEffects.Copy);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error preparing drag files: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== Column Sorting =====

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == sortColumn)
            {
                // Toggle sort order
                sortOrder = sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                sortColumn = e.Column;
                sortOrder = SortOrder.Ascending;
            }

            SortListView();
        }

        private void SortListView()
        {
            if (sortColumn == -1 || sortOrder == SortOrder.None)
                return;

            listView.ListViewItemSorter = new ListViewItemComparer(sortColumn, sortOrder);
            listView.Sort();
        }

        // ===== Search/Filter =====

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterListView(txtSearch.Text);
        }

        private void FilterListView(string searchText)
        {
            listView.BeginUpdate();
            listView.Items.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Show all items
                foreach (ListViewItem item in allItems)
                {
                    listView.Items.Add((ListViewItem)item.Clone());
                }
            }
            else
            {
                // Filter items
                string search = searchText.ToLower();
                foreach (ListViewItem item in allItems)
                {
                    if (item.Text.ToLower().Contains(search))
                    {
                        listView.Items.Add((ListViewItem)item.Clone());
                    }
                }
            }

            listView.EndUpdate();
            UpdateStatusBar();
            UpdateStatistics();
        }

        private void RefreshFileView()
        {
            listView.Items.Clear();
            allItems.Clear();

            // Clear search if switching modes
            if (txtSearch != null)
                txtSearch.Text = "";

            if (currentMode == 0) // Filesystem view
            {
                RefreshFilesystemView();
            }
            else if (currentMode == 1) // XZP content view
            {
                RefreshXzpView();
            }

            // Cache all items for search
            foreach (ListViewItem item in listView.Items)
            {
                allItems.Add((ListViewItem)item.Clone());
            }

            UpdateStatusBar();
            UpdateMenuState();
            UpdateStatistics();
        }

        private void RefreshFilesystemView()
        {
            try
            {
                // Add "Up" directory if not at root
                DirectoryInfo dirInfo = new DirectoryInfo(currentPath);
                if (dirInfo.Parent != null)
                {
                    ListViewItem upItem = new ListViewItem("..");
                    upItem.SubItems.Add("Folder");
                    upItem.SubItems.Add("");
                    upItem.SubItems.Add("");
                    upItem.SubItems.Add("");
                    upItem.ImageIndex = 0;
                    upItem.Tag = "UP_DIR";
                    listView.Items.Add(upItem);
                }

                // Add directories
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    ListViewItem item = new ListViewItem(dir.Name);
                    item.SubItems.Add("Folder");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add(dir.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));

                    // Use system folder icon
                    string iconKey = SystemIconHelper.GetImageKey(dir.Name, true);
                    item.ImageKey = iconKey;
                    item.Tag = dir.FullName;
                    listView.Items.Add(item);
                }

                // Add files
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    ListViewItem item = new ListViewItem(file.Name);
                    string extension = file.Extension.ToLower();

                    if (extension == ".xzp")
                        item.SubItems.Add("XZP Archive");
                    else
                        item.SubItems.Add(extension.TrimStart('.').ToUpper() + " File");

                    item.SubItems.Add(FormatFileSize(file.Length));
                    item.SubItems.Add("");
                    item.SubItems.Add(file.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));

                    // Use system file icon based on extension
                    string iconKey = SystemIconHelper.GetImageKey(file.Name, false);
                    if (imageList.Images.ContainsKey(iconKey))
                        item.ImageKey = iconKey;
                    else
                        item.ImageKey = "txt"; // Default icon

                    item.Tag = file.FullName;
                    listView.Items.Add(item);
                }

                lblCurrentPath.Text = currentPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading directory: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshXzpView()
        {
            try
            {
                // Load XZP metadata
                XzpMetadata metadata = Xuiz.GetMetadata(currentXzpFile);
                currentXzpVersion = metadata.Version;
                currentXzpDataOffset = metadata.DataOffset;
                currentXzpEntryCount = metadata.EntryCount;
                currentXzpFileSize = metadata.FileSize;

                // Add "Up" directory to go back to filesystem view
                ListViewItem upItem = new ListViewItem("..");
                upItem.SubItems.Add("Folder");
                upItem.SubItems.Add("");
                upItem.SubItems.Add("");
                upItem.SubItems.Add("");
                upItem.ImageIndex = 0;
                upItem.Tag = "UP_DIR";
                listView.Items.Add(upItem);

                // Load XZP contents
                List<XuizEntry> entries = Xuiz.GetEntries(currentXzpFile);

                foreach (XuizEntry entry in entries)
                {
                    ListViewItem item = new ListViewItem(entry.Name);
                    item.SubItems.Add(GetFileTypeFromExtension(entry.Name));
                    item.SubItems.Add(FormatFileSize(entry.Size));
                    item.SubItems.Add("0x" + entry.Offset.ToString("X"));
                    item.SubItems.Add("");

                    // Use system file icon based on extension
                    string iconKey = SystemIconHelper.GetImageKey(entry.Name, false);
                    if (imageList.Images.ContainsKey(iconKey))
                        item.ImageKey = iconKey;
                    else
                        item.ImageKey = "txt"; // Default icon

                    item.Tag = entry;
                    listView.Items.Add(item);
                }

                lblCurrentPath.Text = $"{currentXzpFile} (Version {currentXzpVersion}, {currentXzpEntryCount} entries)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading XZP file: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                currentMode = 0;
                currentXzpFile = "";
                RefreshFileView();
            }
        }

        private string GetFileTypeFromExtension(string filename)
        {
            string ext = Path.GetExtension(filename).ToLower();
            switch (ext)
            {
                case ".png": return "PNG Image";
                case ".jpg":
                case ".jpeg": return "JPEG Image";
                case ".xex": return "Xbox Executable";
                case ".xui": return "XUI Interface";
                case ".xur": return "XUR Interface";
                case ".xml": return "XML File";
                case ".txt": return "Text File";
                default: return ext.TrimStart('.').ToUpper() + " File";
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

        private void UpdateStatusBar()
        {
            int itemCount = listView.Items.Count;
            if (itemCount > 0 && listView.Items[0].Tag?.ToString() == "UP_DIR")
                itemCount--;

            lblStatus.Text = $"{itemCount} item(s)";

            if (listView.SelectedItems.Count > 0)
            {
                lblStatus.Text += $" | {listView.SelectedItems.Count} selected";
            }

            if (currentMode == 1)
            {
                lblStatus.Text += $" | XZP v{currentXzpVersion} | Size: {FormatFileSize(currentXzpFileSize)}";
            }
        }

        private void UpdateMenuState()
        {
            bool inXzp = currentMode == 1;
            bool hasXzp = !string.IsNullOrEmpty(currentXzpFile);

            closeArchiveToolStripMenuItem.Enabled = inXzp;
            addFilesToolStripMenuItem.Enabled = inXzp;
            extractAllToolStripMenuItem.Enabled = inXzp;
            convertToolStripMenuItem.Enabled = inXzp;
            archiveDetailsToolStripMenuItem.Enabled = inXzp;
            encryptXZPToolStripMenuItem.Enabled = inXzp;
            btnExtractAll.Enabled = inXzp;
        }

        private void UpdateStatistics()
        {
            try
            {
                int totalFiles = 0;
                long totalSize = 0;
                int selectedFiles = 0;
                long selectedSize = 0;

                // Count all items (excluding UP_DIR)
                foreach (ListViewItem item in listView.Items)
                {
                    if (item.Tag?.ToString() == "UP_DIR")
                        continue;

                    totalFiles++;

                    // Get size
                    long size = 0;
                    if (currentMode == 1) // XZP mode
                    {
                        if (item.Tag is XuizEntry entry)
                        {
                            size = entry.Size;
                        }
                    }
                    else // Filesystem mode
                    {
                        string path = item.Tag?.ToString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            FileInfo fileInfo = new FileInfo(path);
                            size = fileInfo.Length;
                        }
                    }

                    totalSize += size;

                    // Count selected items
                    if (item.Selected)
                    {
                        selectedFiles++;
                        selectedSize += size;
                    }
                }

                // Build statistics string
                string stats = $"Total: {totalFiles} file(s), {FormatFileSize(totalSize)}";

                if (selectedFiles > 0)
                {
                    stats += $"  |  Selected: {selectedFiles} file(s), {FormatFileSize(selectedSize)}";
                }

                if (currentMode == 1 && currentXzpFileSize > 0)
                {
                    long overhead = currentXzpFileSize - totalSize;
                    double overheadPercent = (double)overhead / currentXzpFileSize * 100;
                    stats += $"  |  Overhead: {FormatFileSize(overhead)} ({overheadPercent:0.##}%)";
                }

                lblStats.Text = stats;
            }
            catch
            {
                lblStats.Text = "";
            }
        }

        // ===== Menu Handlers =====

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "XZP Archive|*.xzp|All Files|*.*";
                dialog.Title = "Open XZP Archive";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    OpenXzpFile(dialog.FileName);
                }
            }
        }

        private void closeArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentMode == 1)
            {
                currentMode = 0;
                currentXzpFile = "";
                RefreshFileView();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentMode != 1) return;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "All Files|*.*";
                dialog.Title = "Add Files to XZP Archive";
                dialog.Multiselect = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    AddFilesToArchive(dialog.FileNames);
                }
            }
        }

        private void AddFilesToArchive(string[] files)
        {
            // Add files directly to archive
            AddFilesToArchiveInternal(files);
        }

        private void AddFilesToArchiveInternal(string[] files)
        {
            try
            {
                // Reset "Always Overwrite" setting for new drag & drop operation
                bool sessionOverwrite = alwaysOverwrite;
                List<string> filesToAdd = new List<string>();
                string tempRoot = null;

                foreach (string file in files)
                {
                    string ext = Path.GetExtension(file).ToLower();

                    if (ext == ".xui")
                    {
                        XuiImportMode mode = GetXuiImportMode();
                        switch (mode)
                        {
                            case XuiImportMode.ImportOriginal:
                                filesToAdd.Add(file);
                                break;
                            case XuiImportMode.ConvertOnly:
                                filesToAdd.Add(ConvertXuiToTempXur(file, ref tempRoot));
                                break;
                            case XuiImportMode.Both:
                                filesToAdd.Add(file);
                                filesToAdd.Add(ConvertXuiToTempXur(file, ref tempRoot));
                                break;
                        }
                    }
                    else if (ext == ".xur")
                    {
                        XuiImportMode mode = GetXurImportMode();
                        switch (mode)
                        {
                            case XuiImportMode.ImportOriginal:
                                filesToAdd.Add(file);
                                break;
                            case XuiImportMode.ConvertOnly:
                                filesToAdd.Add(ConvertXurToTempXui(file, ref tempRoot));
                                break;
                            case XuiImportMode.Both:
                                filesToAdd.Add(file);
                                filesToAdd.Add(ConvertXurToTempXui(file, ref tempRoot));
                                break;
                        }
                    }
                    else
                    {
                        filesToAdd.Add(file);
                    }
                }

                if (filesToAdd.Count == 0)
                    return;

                using (Form progressForm = new Form())
                {
                    progressForm.Text = "Adding Files...";
                    progressForm.Size = new Size(400, 120);
                    progressForm.StartPosition = FormStartPosition.CenterParent;
                    progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    progressForm.MaximizeBox = false;
                    progressForm.MinimizeBox = false;

                    Label label = new Label();
                    label.Text = $"Adding {filesToAdd.Count} file(s) to archive...";
                    label.AutoSize = true;
                    label.Location = new Point(20, 30);
                    progressForm.Controls.Add(label);

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += (s, args) =>
                    {
                        Xuiz.AddFiles(currentXzpFile, filesToAdd.ToArray(), (fileName) =>
                        {
                            // Callback for overwrite decision
                            bool overwrite = false;

                            // Use Invoke to show dialog on UI thread
                            progressForm.Invoke((MethodInvoker)delegate
                            {
                                if (sessionOverwrite || alwaysOverwrite)
                                {
                                    overwrite = true;
                                }
                                else
                                {
                                    using (OverwriteDialog dialog = new OverwriteDialog(fileName))
                                    {
                                        DialogResult result = dialog.ShowDialog(progressForm);

                                        if (result == DialogResult.Yes)
                                        {
                                            overwrite = true;
                                            if (dialog.AlwaysOverwrite)
                                            {
                                                alwaysOverwrite = true;
                                                sessionOverwrite = true;
                                            }
                                        }
                                        else if (result == DialogResult.Cancel)
                                        {
                                            throw new OperationCanceledException("Operation cancelled by user");
                                        }
                                    }
                                }
                            });

                            return overwrite;
                        });
                    };
                    worker.RunWorkerCompleted += (s, args) =>
                    {
                        progressForm.Close();

                        if (args.Error != null && !(args.Error is OperationCanceledException))
                        {
                            MessageBox.Show("Error adding files: " + args.Error.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else if (!(args.Error is OperationCanceledException))
                        {
                            MessageBox.Show($"Successfully added {filesToAdd.Count} file(s).", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshFileView();
                        }
                    };

                    worker.RunWorkerAsync();
                    progressForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                    return;

                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private XuiImportMode GetXuiImportMode()
        {
            if (xuiImportMode.HasValue)
                return xuiImportMode.Value;

            using (XuiImportDialog dialog = new XuiImportDialog(false))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (dialog.RememberChoice)
                        xuiImportMode = dialog.SelectedMode;

                    return dialog.SelectedMode;
                }
            }

            throw new OperationCanceledException("Import canceled by user");
        }

        private XuiImportMode GetXurImportMode()
        {
            if (xurImportMode.HasValue)
                return xurImportMode.Value;

            using (XuiImportDialog dialog = new XuiImportDialog(true))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (dialog.RememberChoice)
                        xurImportMode = dialog.SelectedMode;

                    return dialog.SelectedMode;
                }
            }

            throw new OperationCanceledException("Import canceled by user");
        }

        private string ConvertXuiToTempXur(string sourcePath, ref string tempRoot)
        {
            if (string.IsNullOrEmpty(tempRoot))
            {
                tempRoot = Path.Combine(Path.GetTempPath(), "XZPTool", "Converted", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempRoot);
            }

            string fileRoot = Path.Combine(tempRoot, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(fileRoot);

            string fileName = Path.GetFileNameWithoutExtension(sourcePath) + ".xur";
            string targetPath = Path.Combine(fileRoot, fileName);

            if (!XuiConverter.ConvertXuiToXur(sourcePath, targetPath))
                throw new Exception($"Failed to convert XUI to XUR: {Path.GetFileName(sourcePath)}");

            return targetPath;
        }

        private string ConvertXurToTempXui(string sourcePath, ref string tempRoot)
        {
            if (string.IsNullOrEmpty(tempRoot))
            {
                tempRoot = Path.Combine(Path.GetTempPath(), "XZPTool", "Converted", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempRoot);
            }

            string fileRoot = Path.Combine(tempRoot, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(fileRoot);

            string fileName = Path.GetFileNameWithoutExtension(sourcePath) + ".xui";
            string targetPath = Path.Combine(fileRoot, fileName);

            if (!XuiConverter.ConvertXurToXui(sourcePath, targetPath))
                throw new Exception($"Failed to convert XUR to XUI: {Path.GetFileName(sourcePath)}");

            return targetPath;
        }

        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnExtractAll_Click(sender, e);
        }

        private void buildArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnBuild_Click(sender, e);
        }

        private void convertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentMode != 1) return;

            using (ConvertForm convertForm = new ConvertForm(currentXzpFile, currentXzpVersion))
            {
                if (convertForm.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshFileView();
                }
            }
        }

        private void copyArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentMode != 1) return;

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "XZP Archive|*.xzp|All Files|*.*";
                dialog.Title = "Copy XZP Archive";
                dialog.FileName = Path.GetFileName(currentXzpFile);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.Copy(currentXzpFile, dialog.FileName, true);
                        MessageBox.Show("Archive copied successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error copying archive: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void archiveDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentMode != 1) return;

            using (DetailsForm detailsForm = new DetailsForm(currentXzpFile))
            {
                detailsForm.ShowDialog(this);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutForm about = new AboutForm())
            {
                about.ShowDialog(this);
            }
        }

        // ===== Event Handlers =====

        private void listView_DoubleClick(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0) return;

            ListViewItem item = listView.SelectedItems[0];

            if (item.Tag?.ToString() == "UP_DIR")
            {
                NavigateUp();
            }
            else if (currentMode == 0) // Filesystem
            {
                string path = item.Tag.ToString();

                if (Directory.Exists(path))
                {
                    currentPath = path;
                    RefreshFileView();
                }
                else if (File.Exists(path) && path.ToLower().EndsWith(".xzp"))
                {
                    OpenXzpFile(path);
                }
            }
            else if (currentMode == 1) // Inside XZP
            {
                XuizEntry entry = (XuizEntry)item.Tag;
                ViewXzpEntry(entry);
            }
        }

        private void NavigateUp()
        {
            if (currentMode == 1)
            {
                // Exit XZP view
                currentMode = 0;
                currentXzpFile = "";
                RefreshFileView();
            }
            else if (currentMode == 0)
            {
                // Go up one directory
                DirectoryInfo dirInfo = new DirectoryInfo(currentPath);
                if (dirInfo.Parent != null)
                {
                    currentPath = dirInfo.Parent.FullName;
                    RefreshFileView();
                }
            }
        }

        private void OpenXzpFile(string xzpPath)
        {
            currentXzpFile = xzpPath;
            currentMode = 1;
            RefreshFileView();
        }

        private void ViewXzpEntry(XuizEntry entry)
        {
            try
            {
                string ext = Path.GetExtension(entry.Name).ToLower();

                // Handle XUI/XUR files specially
                if (ext == ".xui" || ext == ".xur")
                {
                    // Extract to temp first
                    string tempPath = Path.Combine(Path.GetTempPath(), "XZPTool", entry.Name);
                    Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
                    Xuiz.ExtractEntry(currentXzpFile, entry, tempPath);

                    // Handle with XUI/XUR dialog
                    HandleXuiXurFile(tempPath);
                }
                else
                {
                    // Normal files - extract and open
                    string tempPath = Path.Combine(Path.GetTempPath(), "XZPTool", entry.Name);
                    Directory.CreateDirectory(Path.GetDirectoryName(tempPath));

                    Xuiz.ExtractEntry(currentXzpFile, entry, tempPath);
                    System.Diagnostics.Process.Start(tempPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error viewing file: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2 && listView.SelectedItems.Count == 1)
            {
                StartRename();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedItems();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Back)
            {
                NavigateUp();
                e.Handled = true;
            }
        }

        private void StartRename()
        {
            if (listView.SelectedItems.Count != 1) return;

            ListViewItem item = listView.SelectedItems[0];

            if (item.Tag?.ToString() == "UP_DIR")
                return;

            editingItem = item;
            item.BeginEdit();
        }

        private void listView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label == null || e.Label == editingItem.Text)
            {
                e.CancelEdit = true;
                return;
            }

            string newName = e.Label;

            if (string.IsNullOrWhiteSpace(newName))
            {
                e.CancelEdit = true;
                MessageBox.Show("Name cannot be empty.", "Invalid Name",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (currentMode == 0) // Filesystem rename
                {
                    RenameFilesystemItem(editingItem, newName);
                }
                else if (currentMode == 1) // XZP entry rename
                {
                    RenameXzpEntry(editingItem, newName);
                }
            }
            catch (Exception ex)
            {
                e.CancelEdit = true;
                MessageBox.Show("Error renaming: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenameFilesystemItem(ListViewItem item, string newName)
        {
            string oldPath = item.Tag.ToString();
            string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);

            if (File.Exists(oldPath))
            {
                if (File.Exists(newPath))
                {
                    throw new Exception("A file with that name already exists.");
                }
                File.Move(oldPath, newPath);
            }
            else if (Directory.Exists(oldPath))
            {
                if (Directory.Exists(newPath))
                {
                    throw new Exception("A directory with that name already exists.");
                }
                Directory.Move(oldPath, newPath);
            }

            RefreshFileView();
        }

        private void RenameXzpEntry(ListViewItem item, string newName)
        {
            XuizEntry entry = (XuizEntry)item.Tag;

            using (Form progressForm = new Form())
            {
                progressForm.Text = "Renaming...";
                progressForm.Size = new Size(400, 100);
                progressForm.StartPosition = FormStartPosition.CenterParent;
                progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                progressForm.MaximizeBox = false;
                progressForm.MinimizeBox = false;

                Label label = new Label();
                label.Text = "Renaming file in XZP archive...";
                label.AutoSize = true;
                label.Location = new Point(20, 20);
                progressForm.Controls.Add(label);

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (s, args) =>
                {
                    Xuiz.RenameEntry(currentXzpFile, entry.Name, newName);
                };
                worker.RunWorkerCompleted += (s, args) =>
                {
                    progressForm.Close();
                    if (args.Error != null)
                    {
                        throw args.Error;
                    }
                    RefreshFileView();
                };

                worker.RunWorkerAsync();
                progressForm.ShowDialog(this);
            }
        }

        // ===== Context Menu Handlers =====

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            bool hasSelection = listView.SelectedItems.Count > 0;
            bool singleSelection = listView.SelectedItems.Count == 1;
            bool isUpDir = hasSelection && listView.SelectedItems[0].Tag?.ToString() == "UP_DIR";

            renameToolStripMenuItem.Enabled = singleSelection && !isUpDir;
            deleteToolStripMenuItem.Enabled = hasSelection && !isUpDir;
            extractToolStripMenuItem.Enabled = currentMode == 1 && hasSelection && !isUpDir;
            viewToolStripMenuItem.Enabled = singleSelection && !isUpDir;

            openContextToolStripMenuItem.Visible = currentMode == 0;
            openContextToolStripMenuItem.Enabled = singleSelection && !isUpDir;
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartRename();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedItems();
        }

        private void DeleteSelectedItems()
        {
            if (listView.SelectedItems.Count == 0) return;

            List<ListViewItem> itemsToDelete = new List<ListViewItem>();

            foreach (ListViewItem item in listView.SelectedItems)
            {
                if (item.Tag?.ToString() != "UP_DIR")
                    itemsToDelete.Add(item);
            }

            if (itemsToDelete.Count == 0) return;

            string message = itemsToDelete.Count == 1
                ? $"Are you sure you want to delete '{itemsToDelete[0].Text}'?"
                : $"Are you sure you want to delete {itemsToDelete.Count} items?";

            DialogResult result = MessageBox.Show(message, "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            try
            {
                if (currentMode == 0) // Filesystem delete
                {
                    DeleteFilesystemItems(itemsToDelete);
                }
                else if (currentMode == 1) // XZP entry delete
                {
                    DeleteXzpEntries(itemsToDelete);
                }

                RefreshFileView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteFilesystemItems(List<ListViewItem> items)
        {
            foreach (ListViewItem item in items)
            {
                string path = item.Tag.ToString();

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        }

        private void DeleteXzpEntries(List<ListViewItem> items)
        {
            List<string> entriesToDelete = new List<string>();

            foreach (ListViewItem item in items)
            {
                XuizEntry entry = (XuizEntry)item.Tag;
                entriesToDelete.Add(entry.Name);
            }

            using (Form progressForm = new Form())
            {
                progressForm.Text = "Deleting...";
                progressForm.Size = new Size(400, 100);
                progressForm.StartPosition = FormStartPosition.CenterParent;
                progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                progressForm.MaximizeBox = false;
                progressForm.MinimizeBox = false;

                Label label = new Label();
                label.Text = $"Deleting {entriesToDelete.Count} item(s) from XZP archive...";
                label.AutoSize = true;
                label.Location = new Point(20, 20);
                progressForm.Controls.Add(label);

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (s, args) =>
                {
                    Xuiz.DeleteEntries(currentXzpFile, entriesToDelete);
                };
                worker.RunWorkerCompleted += (s, args) =>
                {
                    progressForm.Close();
                    if (args.Error != null)
                    {
                        throw args.Error;
                    }
                };

                worker.RunWorkerAsync();
                progressForm.ShowDialog(this);
            }
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentMode != 1 || listView.SelectedItems.Count == 0) return;

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select extraction destination";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    List<XuizEntry> entries = new List<XuizEntry>();

                    foreach (ListViewItem item in listView.SelectedItems)
                    {
                        if (item.Tag?.ToString() != "UP_DIR")
                            entries.Add((XuizEntry)item.Tag);
                    }

                    ExtractEntries(entries, dialog.SelectedPath);
                }
            }
        }

        private void ExtractEntries(List<XuizEntry> entries, string destinationPath)
        {
            using (Form progressForm = new Form())
            {
                progressForm.Text = "Extracting...";
                progressForm.Size = new Size(400, 120);
                progressForm.StartPosition = FormStartPosition.CenterParent;
                progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                progressForm.MaximizeBox = false;
                progressForm.MinimizeBox = false;

                Label label = new Label();
                label.Text = "Extracting files...";
                label.AutoSize = true;
                label.Location = new Point(20, 20);
                progressForm.Controls.Add(label);

                ProgressBar progressBar = new ProgressBar();
                progressBar.Location = new Point(20, 50);
                progressBar.Size = new Size(350, 23);
                progressBar.Maximum = entries.Count;
                progressForm.Controls.Add(progressBar);

                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += (s, args) =>
                {
                    int count = 0;
                    foreach (XuizEntry entry in entries)
                    {
                        string outputPath = Path.Combine(destinationPath, entry.Name);
                        Xuiz.ExtractEntry(currentXzpFile, entry, outputPath);
                        count++;
                        worker.ReportProgress(count);
                    }
                };
                worker.ProgressChanged += (s, args) =>
                {
                    progressBar.Value = args.ProgressPercentage;
                    label.Text = $"Extracting files... ({args.ProgressPercentage}/{entries.Count})";
                };
                worker.RunWorkerCompleted += (s, args) =>
                {
                    progressForm.Close();
                    if (args.Error != null)
                    {
                        MessageBox.Show("Error extracting: " + args.Error.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Successfully extracted {entries.Count} file(s).", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };

                worker.RunWorkerAsync();
                progressForm.ShowDialog(this);
            }
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count != 1) return;

            ListViewItem item = listView.SelectedItems[0];

            if (item.Tag?.ToString() == "UP_DIR") return;

            if (currentMode == 1)
            {
                XuizEntry entry = (XuizEntry)item.Tag;
                ViewXzpEntry(entry);
            }
            else
            {
                string path = item.Tag.ToString();
                if (File.Exists(path))
                {
                    System.Diagnostics.Process.Start(path);
                }
            }
        }

        private void openContextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count != 1) return;

            ListViewItem item = listView.SelectedItems[0];

            if (item.Tag?.ToString() == "UP_DIR") return;

            string path = item.Tag.ToString();

            if (Directory.Exists(path))
            {
                currentPath = path;
                RefreshFileView();
            }
            else if (File.Exists(path) && path.ToLower().EndsWith(".xzp"))
            {
                OpenXzpFile(path);
            }
            else if (File.Exists(path))
            {
                System.Diagnostics.Process.Start(path);
            }
        }

        // ===== Toolbar Handlers =====

        private void btnUp_Click(object sender, EventArgs e)
        {
            NavigateUp();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshFileView();
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            using (BuildForm buildForm = new BuildForm())
            {
                buildForm.ShowDialog(this);
            }
        }

        private void btnExtractAll_Click(object sender, EventArgs e)
        {
            if (currentMode != 1) return;

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select extraction destination";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    List<XuizEntry> allEntries = Xuiz.GetEntries(currentXzpFile);
                    ExtractEntries(allEntries, dialog.SelectedPath);
                }
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateStatusBar();
            UpdateStatistics();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = "XZP Tool v3.0 - Advanced XZP Archive Manager";

            // Handle file passed via command line (file association)
            if (!string.IsNullOrEmpty(Program.StartupFile))
            {
                HandleStartupFile(Program.StartupFile);
            }
        }

        private void HandleStartupFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            string ext = Path.GetExtension(filePath).ToLower();

            // Handle XUI/XUR files
            if (ext == ".xui" || ext == ".xur")
            {
                HandleXuiXurFile(filePath);
            }
            // Handle XZP files
            else if (ext == ".xzp")
            {
                OpenXzpFile(filePath);
            }
        }

        private void HandleXuiXurFile(string filePath)
        {
            bool isXur = Path.GetExtension(filePath).ToLower() == ".xur";

            // Check if user has a saved preference for this session
            if (xuiOpenMode.HasValue)
            {
                ExecuteXuiOpenMode(filePath, isXur, xuiOpenMode.Value);
                return;
            }

            // Show dialog to ask user what to do
            using (XuiOpenDialog dialog = new XuiOpenDialog(Path.GetFileName(filePath), isXur))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (dialog.RememberChoice)
                    {
                        xuiOpenMode = dialog.SelectedMode;
                    }

                    ExecuteXuiOpenMode(filePath, isXur, dialog.SelectedMode);
                }
            }
        }

        private void ExecuteXuiOpenMode(string filePath, bool isXur, XuiOpenDialog.OpenMode mode)
        {
            try
            {
                string openPath = filePath;
                if (isXur && mode != XuiOpenDialog.OpenMode.AddToArchive)
                {
                    openPath = ConvertXurToTempXuiForOpen(filePath);
                    if (string.IsNullOrEmpty(openPath))
                        return;
                }

                switch (mode)
                {
                    case XuiOpenDialog.OpenMode.ExternalProgram:
                        System.Diagnostics.Process.Start(openPath);
                        break;

                    case XuiOpenDialog.OpenMode.InternalViewer:
                        using (XuiViewerForm viewer = new XuiViewerForm(openPath))
                        {
                            viewer.ShowDialog(this);
                        }
                        break;

                    case XuiOpenDialog.OpenMode.AddToArchive:
                        if (currentMode == 1)
                        {
                            AddFilesToArchive(new string[] { filePath });
                        }
                        else
                        {
                            MessageBox.Show("Please open an XZP archive first.", "No Archive Open",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ConvertXurToTempXuiForOpen(string xurPath)
        {
            string tempRoot = Path.Combine(Path.GetTempPath(), "XZPTool", "XurToXui", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRoot);

            string fileName = Path.GetFileNameWithoutExtension(xurPath) + ".xui";
            string targetPath = Path.Combine(tempRoot, fileName);

            if (XuiConverter.ConvertXurToXui(xurPath, targetPath))
                return targetPath;

            MessageBox.Show("Failed to convert XUR to XUI for viewing.", "Conversion Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        private void encryptXZPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentMode != 1 || string.IsNullOrEmpty(currentXzpFile))
            {
                MessageBox.Show("Please open an XZP archive first.", "No Archive Loaded",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (EncryptForm encryptForm = new EncryptForm(currentXzpFile))
                {
                    encryptForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open encryption form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (SettingsForm settingsForm = new SettingsForm())
                {
                    if (settingsForm.ShowDialog(this) == DialogResult.OK)
                    {
                        // Settings have been saved
                        // Update alwaysOverwrite from settings
                        alwaysOverwrite = AppSettings.Instance.AlwaysOverwrite;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// Comparer for sorting ListView columns
    /// </summary>
    public class ListViewItemComparer : System.Collections.IComparer
    {
        private int col;
        private SortOrder order;

        public ListViewItemComparer(int column, SortOrder sortOrder)
        {
            col = column;
            order = sortOrder;
        }

        public int Compare(object x, object y)
        {
            int returnVal = -1;
            ListViewItem itemX = (ListViewItem)x;
            ListViewItem itemY = (ListViewItem)y;

            // Don't sort the "UP" item
            if (itemX.Tag?.ToString() == "UP_DIR")
                return -1;
            if (itemY.Tag?.ToString() == "UP_DIR")
                return 1;

            string textX = itemX.SubItems.Count > col ? itemX.SubItems[col].Text : "";
            string textY = itemY.SubItems.Count > col ? itemY.SubItems[col].Text : "";

            // Try numeric comparison for size and offset columns
            if (col == 2 || col == 3) // Size or Offset columns
            {
                long numX, numY;
                if (TryParseSize(textX, out numX) && TryParseSize(textY, out numY))
                {
                    returnVal = numX.CompareTo(numY);
                }
                else
                {
                    returnVal = String.Compare(textX, textY);
                }
            }
            else
            {
                returnVal = String.Compare(textX, textY);
            }

            if (order == SortOrder.Descending)
                returnVal *= -1;

            return returnVal;
        }

        private bool TryParseSize(string text, out long result)
        {
            result = 0;

            if (string.IsNullOrEmpty(text))
                return false;

            // Handle hex values (Offset column)
            if (text.StartsWith("0x"))
            {
                return long.TryParse(text.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out result);
            }

            // Handle size strings like "1.5 MB"
            string[] parts = text.Split(' ');
            if (parts.Length == 2)
            {
                double value;
                if (double.TryParse(parts[0], out value))
                {
                    string unit = parts[1].ToUpper();
                    switch (unit)
                    {
                        case "B":
                            result = (long)value;
                            return true;
                        case "KB":
                            result = (long)(value * 1024);
                            return true;
                        case "MB":
                            result = (long)(value * 1024 * 1024);
                            return true;
                        case "GB":
                            result = (long)(value * 1024 * 1024 * 1024);
                            return true;
                    }
                }
            }

            return long.TryParse(text, out result);
        }
    }
}
