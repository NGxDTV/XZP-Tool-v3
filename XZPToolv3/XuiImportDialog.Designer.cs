namespace XZPToolv3
{
    partial class XuiImportDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XuiImportDialog));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioBoth = new System.Windows.Forms.RadioButton();
            this.radioConvertToXur = new System.Windows.Forms.RadioButton();
            this.radioXuiOnly = new System.Windows.Forms.RadioButton();
            this.chkRemember = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioBoth);
            this.groupBox1.Controls.Add(this.radioConvertToXur);
            this.groupBox1.Controls.Add(this.radioXuiOnly);
            this.groupBox1.Location = new System.Drawing.Point(12, 50);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(460, 110);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Import Mode";
            // 
            // radioBoth
            // 
            this.radioBoth.AutoSize = true;
            this.radioBoth.Location = new System.Drawing.Point(15, 75);
            this.radioBoth.Name = "radioBoth";
            this.radioBoth.Size = new System.Drawing.Size(334, 17);
            this.radioBoth.TabIndex = 2;
            this.radioBoth.Text = "Import both XUI and XUR (XUI will be converted and both added)";
            this.radioBoth.UseVisualStyleBackColor = true;
            // 
            // radioConvertToXur
            // 
            this.radioConvertToXur.AutoSize = true;
            this.radioConvertToXur.Location = new System.Drawing.Point(15, 50);
            this.radioConvertToXur.Name = "radioConvertToXur";
            this.radioConvertToXur.Size = new System.Drawing.Size(264, 17);
            this.radioConvertToXur.TabIndex = 1;
            this.radioConvertToXur.Text = "Convert to XUR and import only XUR (discard XUI)";
            this.radioConvertToXur.UseVisualStyleBackColor = true;
            // 
            // radioXuiOnly
            // 
            this.radioXuiOnly.AutoSize = true;
            this.radioXuiOnly.Checked = true;
            this.radioXuiOnly.Location = new System.Drawing.Point(15, 25);
            this.radioXuiOnly.Name = "radioXuiOnly";
            this.radioXuiOnly.Size = new System.Drawing.Size(173, 17);
            this.radioXuiOnly.TabIndex = 0;
            this.radioXuiOnly.TabStop = true;
            this.radioXuiOnly.Text = "Import XUI only (no conversion)";
            this.radioXuiOnly.UseVisualStyleBackColor = true;
            // 
            // chkRemember
            // 
            this.chkRemember.AutoSize = true;
            this.chkRemember.Location = new System.Drawing.Point(12, 175);
            this.chkRemember.Name = "chkRemember";
            this.chkRemember.Size = new System.Drawing.Size(281, 17);
            this.chkRemember.TabIndex = 1;
            this.chkRemember.Text = "Remember my choice for this session (don\'t ask again)";
            this.chkRemember.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(316, 210);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(397, 210);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(358, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "You are importing XUI file(s). How would you like to handle the XUI import?";
            // 
            // XuiImportDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 245);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.chkRemember);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "XuiImportDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "XUI Import Options";
            this.Load += new System.EventHandler(this.XuiImportDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioBoth;
        private System.Windows.Forms.RadioButton radioConvertToXur;
        private System.Windows.Forms.RadioButton radioXuiOnly;
        private System.Windows.Forms.CheckBox chkRemember;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
    }
}
