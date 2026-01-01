using System;
using System.IO;
using System.Windows.Forms;

namespace XZPToolv3
{
    public partial class XusEditorForm : Form
    {
        private string currentPath;
        private XusFile currentFile;
        private bool isDirty;
        private bool suppressEvents;

        public XusEditorForm()
        {
            InitializeComponent();
        }

        public XusEditorForm(string path) : this()
        {
            currentPath = path;
        }

        private void XusEditorForm_Load(object sender, EventArgs e)
        {
            cmbKeyMode.Items.Clear();
            cmbKeyMode.Items.Add("String");
            cmbKeyMode.Items.Add("UInt32");
            cmbKeyMode.Items.Add("Index");

            if (!string.IsNullOrEmpty(currentPath) && File.Exists(currentPath))
            {
                LoadXus(currentPath);
            }
            else
            {
                NewFile();
            }
        }

        private void XusEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ConfirmDiscardChanges())
                e.Cancel = true;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            NewFile();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardChanges())
                return;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "XUS Files|*.xus|All Files|*.*";
                dialog.Title = "Open XUS File";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    LoadXus(dialog.FileName);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentPath))
            {
                btnSaveAs_Click(sender, e);
                return;
            }

            SaveXus(currentPath);
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "XUS Files|*.xus|All Files|*.*";
                dialog.Title = "Save XUS File";
                dialog.DefaultExt = "xus";
                if (!string.IsNullOrEmpty(currentPath))
                    dialog.FileName = Path.GetFileName(currentPath);

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    SaveXus(dialog.FileName);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            gridEntries.Rows.Add();
            MarkDirty();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (gridEntries.SelectedRows.Count == 0)
                return;

            foreach (DataGridViewRow row in gridEntries.SelectedRows)
            {
                if (!row.IsNewRow)
                    gridEntries.Rows.Remove(row);
            }

            MarkDirty();
        }

        private void cmbKeyMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateKeyModeUI();
            MarkDirty();
        }

        private void gridEntries_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (suppressEvents)
                return;
            MarkDirty();
        }

        private void gridEntries_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (suppressEvents)
                return;
            UpdateIndexColumn();
            MarkDirty();
        }

        private void gridEntries_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (suppressEvents)
                return;
            UpdateIndexColumn();
            MarkDirty();
        }

        private void gridEntries_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (gridEntries.IsCurrentCellDirty)
                gridEntries.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void NewFile()
        {
            if (!ConfirmDiscardChanges())
                return;

            suppressEvents = true;
            gridEntries.Rows.Clear();
            currentFile = new XusFile();
            currentFile.SetKeyMode(XusKeyMode.String);
            cmbKeyMode.SelectedIndex = (int)XusKeyMode.String;
            currentPath = "";
            suppressEvents = false;

            UpdateKeyModeUI();
            isDirty = false;
            UpdateTitle();
        }

        private void LoadXus(string path)
        {
            try
            {
                currentFile = XusFile.Load(path);
                currentPath = path;
                PopulateGrid(currentFile);
                isDirty = false;
                UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load XUS:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveXus(string path)
        {
            if (!TryBuildFile(out XusFile file))
                return;

            try
            {
                file.Save(path);
                currentFile = file;
                currentPath = path;
                isDirty = false;
                UpdateTitle();
                MessageBox.Show("XUS saved successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save XUS:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateGrid(XusFile file)
        {
            suppressEvents = true;
            gridEntries.Rows.Clear();

            foreach (var entry in file.Entries)
            {
                int idx = gridEntries.Rows.Add();
                var row = gridEntries.Rows[idx];
                row.Cells[colIndex.Name].Value = entry.IndexKey.ToString();
                row.Cells[colUInt32.Name].Value = $"0x{entry.UInt32Key:X8}";
                row.Cells[colString.Name].Value = entry.StringKey ?? "";
                row.Cells[colValue.Name].Value = entry.Value ?? "";
            }

            cmbKeyMode.SelectedIndex = (int)file.KeyMode;
            suppressEvents = false;
            UpdateKeyModeUI();
        }

        private bool TryBuildFile(out XusFile file)
        {
            file = currentFile ?? new XusFile();
            var mode = GetSelectedMode();
            file.SetKeyMode(mode);
            file.Entries.Clear();

            foreach (DataGridViewRow row in gridEntries.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string value = Convert.ToString(row.Cells[colValue.Name].Value) ?? "";

                if (mode == XusKeyMode.Index)
                {
                    file.Entries.Add(new XusEntry
                    {
                        IndexKey = file.Entries.Count,
                        Value = value
                    });
                    continue;
                }

                if (mode == XusKeyMode.UInt32)
                {
                    string keyText = Convert.ToString(row.Cells[colUInt32.Name].Value) ?? "";
                    if (!TryParseUInt32(keyText, out uint key))
                    {
                        MessageBox.Show("Invalid UInt32 key. Use decimal or 0xHEX.", "Invalid Key",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    file.Entries.Add(new XusEntry { UInt32Key = key, Value = value });
                    continue;
                }

                string strKey = Convert.ToString(row.Cells[colString.Name].Value) ?? "";
                file.Entries.Add(new XusEntry { StringKey = strKey, Value = value });
            }

            return true;
        }

        private void UpdateKeyModeUI()
        {
            var mode = GetSelectedMode();
            colIndex.Visible = mode == XusKeyMode.Index;
            colUInt32.Visible = mode == XusKeyMode.UInt32;
            colString.Visible = mode == XusKeyMode.String;
            colIndex.ReadOnly = true;
            UpdateIndexColumn();
        }

        private void UpdateIndexColumn()
        {
            if (GetSelectedMode() != XusKeyMode.Index)
                return;

            for (int i = 0; i < gridEntries.Rows.Count; i++)
            {
                var row = gridEntries.Rows[i];
                if (row.IsNewRow)
                    continue;
                row.Cells[colIndex.Name].Value = i.ToString();
            }
        }

        private XusKeyMode GetSelectedMode()
        {
            if (cmbKeyMode.SelectedIndex < 0)
                return XusKeyMode.String;
            return (XusKeyMode)cmbKeyMode.SelectedIndex;
        }

        private static bool TryParseUInt32(string text, out uint value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return uint.TryParse(text.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out value);

            return uint.TryParse(text, out value);
        }

        private void MarkDirty()
        {
            if (suppressEvents)
                return;
            isDirty = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            string name = string.IsNullOrEmpty(currentPath) ? "Untitled.xus" : Path.GetFileName(currentPath);
            Text = isDirty ? $"XUS Editor - {name} *" : $"XUS Editor - {name}";
        }

        private bool ConfirmDiscardChanges()
        {
            if (!isDirty)
                return true;

            var result = MessageBox.Show("Discard unsaved changes?", "Unsaved Changes",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }
    }
}
