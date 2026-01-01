namespace XZPToolv3
{
    partial class XuiOpenDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XuiOpenDialog));
            this.lblFileName = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioArchive = new System.Windows.Forms.RadioButton();
            this.radioInternal = new System.Windows.Forms.RadioButton();
            this.radioExternal = new System.Windows.Forms.RadioButton();
            this.chkRemember = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.Location = new System.Drawing.Point(12, 15);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(82, 13);
            this.lblFileName.TabIndex = 0;
            this.lblFileName.Text = "File: filename";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioArchive);
            this.groupBox1.Controls.Add(this.radioInternal);
            this.groupBox1.Controls.Add(this.radioExternal);
            this.groupBox1.Location = new System.Drawing.Point(12, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(460, 110);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "How would you like to open this file?";
            // 
            // radioArchive
            // 
            this.radioArchive.AutoSize = true;
            this.radioArchive.Location = new System.Drawing.Point(15, 75);
            this.radioArchive.Name = "radioArchive";
            this.radioArchive.Size = new System.Drawing.Size(207, 17);
            this.radioArchive.TabIndex = 2;
            this.radioArchive.Text = "Add to current XZP archive (if opened)";
            this.radioArchive.UseVisualStyleBackColor = true;
            // 
            // radioInternal
            // 
            this.radioInternal.AutoSize = true;
            this.radioInternal.Checked = true;
            this.radioInternal.Location = new System.Drawing.Point(15, 50);
            this.radioInternal.Name = "radioInternal";
            this.radioInternal.Size = new System.Drawing.Size(168, 17);
            this.radioInternal.TabIndex = 1;
            this.radioInternal.TabStop = true;
            this.radioInternal.Text = "View in XZP Tool (XUI viewer)";
            this.radioInternal.UseVisualStyleBackColor = true;
            // 
            // radioExternal
            // 
            this.radioExternal.AutoSize = true;
            this.radioExternal.Location = new System.Drawing.Point(15, 25);
            this.radioExternal.Name = "radioExternal";
            this.radioExternal.Size = new System.Drawing.Size(213, 17);
            this.radioExternal.TabIndex = 0;
            this.radioExternal.Text = "Open in external program (default editor)";
            this.radioExternal.UseVisualStyleBackColor = true;
            // 
            // chkRemember
            // 
            this.chkRemember.AutoSize = true;
            this.chkRemember.Location = new System.Drawing.Point(12, 170);
            this.chkRemember.Name = "chkRemember";
            this.chkRemember.Size = new System.Drawing.Size(281, 17);
            this.chkRemember.TabIndex = 2;
            this.chkRemember.Text = "Remember my choice for this session (don\'t ask again)";
            this.chkRemember.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(316, 205);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(397, 205);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // XuiOpenDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 240);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.chkRemember);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblFileName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "XuiOpenDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open XUI/XUR File";
            this.Load += new System.EventHandler(this.XuiOpenDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioArchive;
        private System.Windows.Forms.RadioButton radioInternal;
        private System.Windows.Forms.RadioButton radioExternal;
        private System.Windows.Forms.CheckBox chkRemember;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
