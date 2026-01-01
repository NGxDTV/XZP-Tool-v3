namespace XZPToolv3
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.btnResetKey = new System.Windows.Forms.Button();
            this.txtMasterKey = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkAlwaysOverwrite = new System.Windows.Forms.CheckBox();
            this.groupBoxXuiExtensions = new System.Windows.Forms.GroupBox();
            this.btnRemoveXuiExtension = new System.Windows.Forms.Button();
            this.btnAddXuiExtension = new System.Windows.Forms.Button();
            this.lstXuiExtensions = new System.Windows.Forms.ListBox();
            this.tabTemplates = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.txtFooter = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtLineSuffix = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtByteSeparator = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtByteFormat = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.numBytesPerLine = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.txtLinePrefix = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtHeader = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTemplateName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSaveTemplate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnDeleteTemplate = new System.Windows.Forms.Button();
            this.btnNewTemplate = new System.Windows.Forms.Button();
            this.lstTemplates = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.groupBoxXuiExtensions.SuspendLayout();
            this.tabTemplates.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBytesPerLine)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabTemplates);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(784, 511);
            this.tabControl1.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.btnResetKey);
            this.tabGeneral.Controls.Add(this.txtMasterKey);
            this.tabGeneral.Controls.Add(this.label2);
            this.tabGeneral.Controls.Add(this.chkAlwaysOverwrite);
            this.tabGeneral.Controls.Add(this.groupBoxXuiExtensions);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(776, 485);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // btnResetKey
            // 
            this.btnResetKey.Location = new System.Drawing.Point(550, 53);
            this.btnResetKey.Name = "btnResetKey";
            this.btnResetKey.Size = new System.Drawing.Size(100, 23);
            this.btnResetKey.TabIndex = 3;
            this.btnResetKey.Text = "Reset to Default";
            this.btnResetKey.UseVisualStyleBackColor = true;
            this.btnResetKey.Click += new System.EventHandler(this.btnResetKey_Click);
            // 
            // txtMasterKey
            // 
            this.txtMasterKey.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMasterKey.Location = new System.Drawing.Point(20, 55);
            this.txtMasterKey.Name = "txtMasterKey";
            this.txtMasterKey.Size = new System.Drawing.Size(524, 22);
            this.txtMasterKey.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(340, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Master Key for Encryption (hex bytes separated by spaces or commas):";
            // 
            // chkAlwaysOverwrite
            // 
            this.chkAlwaysOverwrite.AutoSize = true;
            this.chkAlwaysOverwrite.Location = new System.Drawing.Point(20, 15);
            this.chkAlwaysOverwrite.Name = "chkAlwaysOverwrite";
            this.chkAlwaysOverwrite.Size = new System.Drawing.Size(238, 17);
            this.chkAlwaysOverwrite.TabIndex = 0;
            this.chkAlwaysOverwrite.Text = "Always overwrite files without asking (default)";
            this.chkAlwaysOverwrite.UseVisualStyleBackColor = true;
            // 
            // groupBoxXuiExtensions
            // 
            this.groupBoxXuiExtensions.Controls.Add(this.btnRemoveXuiExtension);
            this.groupBoxXuiExtensions.Controls.Add(this.btnAddXuiExtension);
            this.groupBoxXuiExtensions.Controls.Add(this.lstXuiExtensions);
            this.groupBoxXuiExtensions.Location = new System.Drawing.Point(20, 95);
            this.groupBoxXuiExtensions.Name = "groupBoxXuiExtensions";
            this.groupBoxXuiExtensions.Size = new System.Drawing.Size(730, 160);
            this.groupBoxXuiExtensions.TabIndex = 4;
            this.groupBoxXuiExtensions.TabStop = false;
            this.groupBoxXuiExtensions.Text = "XUI Extensions (optional)";
            // 
            // btnRemoveXuiExtension
            // 
            this.btnRemoveXuiExtension.Location = new System.Drawing.Point(630, 55);
            this.btnRemoveXuiExtension.Name = "btnRemoveXuiExtension";
            this.btnRemoveXuiExtension.Size = new System.Drawing.Size(85, 23);
            this.btnRemoveXuiExtension.TabIndex = 2;
            this.btnRemoveXuiExtension.Text = "Remove";
            this.btnRemoveXuiExtension.UseVisualStyleBackColor = true;
            this.btnRemoveXuiExtension.Click += new System.EventHandler(this.btnRemoveXuiExtension_Click);
            // 
            // btnAddXuiExtension
            // 
            this.btnAddXuiExtension.Location = new System.Drawing.Point(630, 26);
            this.btnAddXuiExtension.Name = "btnAddXuiExtension";
            this.btnAddXuiExtension.Size = new System.Drawing.Size(85, 23);
            this.btnAddXuiExtension.TabIndex = 1;
            this.btnAddXuiExtension.Text = "Add...";
            this.btnAddXuiExtension.UseVisualStyleBackColor = true;
            this.btnAddXuiExtension.Click += new System.EventHandler(this.btnAddXuiExtension_Click);
            // 
            // lstXuiExtensions
            // 
            this.lstXuiExtensions.FormattingEnabled = true;
            this.lstXuiExtensions.HorizontalScrollbar = true;
            this.lstXuiExtensions.Location = new System.Drawing.Point(15, 26);
            this.lstXuiExtensions.Name = "lstXuiExtensions";
            this.lstXuiExtensions.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstXuiExtensions.Size = new System.Drawing.Size(600, 121);
            this.lstXuiExtensions.TabIndex = 0;
            // 
            // tabTemplates
            // 
            this.tabTemplates.Controls.Add(this.groupBox2);
            this.tabTemplates.Controls.Add(this.groupBox1);
            this.tabTemplates.Location = new System.Drawing.Point(4, 22);
            this.tabTemplates.Name = "tabTemplates";
            this.tabTemplates.Padding = new System.Windows.Forms.Padding(3);
            this.tabTemplates.Size = new System.Drawing.Size(776, 485);
            this.tabTemplates.TabIndex = 1;
            this.tabTemplates.Text = "Output Templates";
            this.tabTemplates.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btnPreview);
            this.groupBox2.Controls.Add(this.txtFooter);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.txtLineSuffix);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.txtByteSeparator);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.txtByteFormat);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.numBytesPerLine);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.txtLinePrefix);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.txtHeader);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtTemplateName);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btnSaveTemplate);
            this.groupBox2.Location = new System.Drawing.Point(220, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(548, 473);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Template Editor";
            // 
            // btnPreview
            // 
            this.btnPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPreview.Location = new System.Drawing.Point(100, 438);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(80, 25);
            this.btnPreview.TabIndex = 17;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // txtFooter
            // 
            this.txtFooter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFooter.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFooter.Location = new System.Drawing.Point(15, 375);
            this.txtFooter.Multiline = true;
            this.txtFooter.Name = "txtFooter";
            this.txtFooter.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtFooter.Size = new System.Drawing.Size(520, 57);
            this.txtFooter.TabIndex = 16;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 359);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(40, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = "Footer:";
            // 
            // txtLineSuffix
            // 
            this.txtLineSuffix.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLineSuffix.Location = new System.Drawing.Point(15, 331);
            this.txtLineSuffix.Name = "txtLineSuffix";
            this.txtLineSuffix.Size = new System.Drawing.Size(200, 22);
            this.txtLineSuffix.TabIndex = 14;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 315);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(105, 13);
            this.label8.TabIndex = 13;
            this.label8.Text = "Line Suffix (e.g., \",\"):";
            // 
            // txtByteSeparator
            // 
            this.txtByteSeparator.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtByteSeparator.Location = new System.Drawing.Point(15, 287);
            this.txtByteSeparator.Name = "txtByteSeparator";
            this.txtByteSeparator.Size = new System.Drawing.Size(200, 22);
            this.txtByteSeparator.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 271);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(129, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Byte Separator (e.g., \", \"):";
            // 
            // txtByteFormat
            // 
            this.txtByteFormat.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtByteFormat.Location = new System.Drawing.Point(15, 243);
            this.txtByteFormat.Name = "txtByteFormat";
            this.txtByteFormat.Size = new System.Drawing.Size(200, 22);
            this.txtByteFormat.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 227);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(150, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Byte Format (e.g., \"0x{0:X2}\"):";
            // 
            // numBytesPerLine
            // 
            this.numBytesPerLine.Location = new System.Drawing.Point(15, 199);
            this.numBytesPerLine.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.numBytesPerLine.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numBytesPerLine.Name = "numBytesPerLine";
            this.numBytesPerLine.Size = new System.Drawing.Size(120, 20);
            this.numBytesPerLine.TabIndex = 8;
            this.numBytesPerLine.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 183);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Bytes Per Line:";
            // 
            // txtLinePrefix
            // 
            this.txtLinePrefix.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLinePrefix.Location = new System.Drawing.Point(15, 155);
            this.txtLinePrefix.Name = "txtLinePrefix";
            this.txtLinePrefix.Size = new System.Drawing.Size(200, 22);
            this.txtLinePrefix.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 139);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Line Prefix (e.g., \"\\t\"):";
            // 
            // txtHeader
            // 
            this.txtHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeader.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHeader.Location = new System.Drawing.Point(15, 73);
            this.txtHeader.Multiline = true;
            this.txtHeader.Name = "txtHeader";
            this.txtHeader.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtHeader.Size = new System.Drawing.Size(520, 60);
            this.txtHeader.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(166, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Header (use {SIZE} for data size):";
            // 
            // txtTemplateName
            // 
            this.txtTemplateName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTemplateName.Location = new System.Drawing.Point(15, 34);
            this.txtTemplateName.Name = "txtTemplateName";
            this.txtTemplateName.Size = new System.Drawing.Size(520, 20);
            this.txtTemplateName.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Template Name:";
            // 
            // btnSaveTemplate
            // 
            this.btnSaveTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSaveTemplate.Location = new System.Drawing.Point(14, 438);
            this.btnSaveTemplate.Name = "btnSaveTemplate";
            this.btnSaveTemplate.Size = new System.Drawing.Size(80, 25);
            this.btnSaveTemplate.TabIndex = 0;
            this.btnSaveTemplate.Text = "Save";
            this.btnSaveTemplate.UseVisualStyleBackColor = true;
            this.btnSaveTemplate.Click += new System.EventHandler(this.btnSaveTemplate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.btnDeleteTemplate);
            this.groupBox1.Controls.Add(this.btnNewTemplate);
            this.groupBox1.Controls.Add(this.lstTemplates);
            this.groupBox1.Location = new System.Drawing.Point(8, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(206, 473);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Templates";
            // 
            // btnDeleteTemplate
            // 
            this.btnDeleteTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteTemplate.Enabled = false;
            this.btnDeleteTemplate.Location = new System.Drawing.Point(99, 438);
            this.btnDeleteTemplate.Name = "btnDeleteTemplate";
            this.btnDeleteTemplate.Size = new System.Drawing.Size(80, 25);
            this.btnDeleteTemplate.TabIndex = 2;
            this.btnDeleteTemplate.Text = "Delete";
            this.btnDeleteTemplate.UseVisualStyleBackColor = true;
            this.btnDeleteTemplate.Click += new System.EventHandler(this.btnDeleteTemplate_Click);
            // 
            // btnNewTemplate
            // 
            this.btnNewTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNewTemplate.Location = new System.Drawing.Point(13, 438);
            this.btnNewTemplate.Name = "btnNewTemplate";
            this.btnNewTemplate.Size = new System.Drawing.Size(80, 25);
            this.btnNewTemplate.TabIndex = 1;
            this.btnNewTemplate.Text = "New";
            this.btnNewTemplate.UseVisualStyleBackColor = true;
            this.btnNewTemplate.Click += new System.EventHandler(this.btnNewTemplate_Click);
            // 
            // lstTemplates
            // 
            this.lstTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstTemplates.FormattingEnabled = true;
            this.lstTemplates.Location = new System.Drawing.Point(13, 19);
            this.lstTemplates.Name = "lstTemplates";
            this.lstTemplates.Size = new System.Drawing.Size(180, 407);
            this.lstTemplates.TabIndex = 0;
            this.lstTemplates.SelectedIndexChanged += new System.EventHandler(this.lstTemplates_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 511);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(784, 50);
            this.panel1.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(697, 15);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(616, 15);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.groupBoxXuiExtensions.ResumeLayout(false);
            this.tabTemplates.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBytesPerLine)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabTemplates;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox chkAlwaysOverwrite;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMasterKey;
        private System.Windows.Forms.Button btnResetKey;
        private System.Windows.Forms.GroupBox groupBoxXuiExtensions;
        private System.Windows.Forms.Button btnRemoveXuiExtension;
        private System.Windows.Forms.Button btnAddXuiExtension;
        private System.Windows.Forms.ListBox lstXuiExtensions;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lstTemplates;
        private System.Windows.Forms.Button btnNewTemplate;
        private System.Windows.Forms.Button btnDeleteTemplate;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSaveTemplate;
        private System.Windows.Forms.TextBox txtTemplateName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtHeader;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtLinePrefix;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numBytesPerLine;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtByteFormat;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtByteSeparator;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtLineSuffix;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtFooter;
        private System.Windows.Forms.Button btnPreview;
    }
}
