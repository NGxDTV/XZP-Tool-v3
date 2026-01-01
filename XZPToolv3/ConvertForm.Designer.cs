namespace XZPToolv3
{
    partial class ConvertForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConvertForm));
            this.lblCurrentVersion = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbVersion3 = new System.Windows.Forms.RadioButton();
            this.rbVersion1 = new System.Windows.Forms.RadioButton();
            this.btnConvert = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCurrentVersion
            // 
            this.lblCurrentVersion.AutoSize = true;
            this.lblCurrentVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentVersion.Location = new System.Drawing.Point(20, 20);
            this.lblCurrentVersion.Name = "lblCurrentVersion";
            this.lblCurrentVersion.Size = new System.Drawing.Size(122, 15);
            this.lblCurrentVersion.TabIndex = 0;
            this.lblCurrentVersion.Text = "Current Version: 3";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbVersion3);
            this.groupBox1.Controls.Add(this.rbVersion1);
            this.groupBox1.Location = new System.Drawing.Point(20, 50);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(360, 80);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Convert To";
            // 
            // rbVersion3
            // 
            this.rbVersion3.AutoSize = true;
            this.rbVersion3.Location = new System.Drawing.Point(20, 30);
            this.rbVersion3.Name = "rbVersion3";
            this.rbVersion3.Size = new System.Drawing.Size(192, 17);
            this.rbVersion3.TabIndex = 0;
            this.rbVersion3.TabStop = true;
            this.rbVersion3.Text = "Version 3 (Modern, Recommended)";
            this.rbVersion3.UseVisualStyleBackColor = true;
            // 
            // rbVersion1
            // 
            this.rbVersion1.AutoSize = true;
            this.rbVersion1.Location = new System.Drawing.Point(20, 53);
            this.rbVersion1.Name = "rbVersion1";
            this.rbVersion1.Size = new System.Drawing.Size(155, 17);
            this.rbVersion1.TabIndex = 1;
            this.rbVersion1.TabStop = true;
            this.rbVersion1.Text = "Version 1 (Legacy, UTF-32)";
            this.rbVersion1.UseVisualStyleBackColor = true;
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(224, 150);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(75, 30);
            this.btnConvert.TabIndex = 2;
            this.btnConvert.Text = "Convert";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(305, 150);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ConvertForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblCurrentVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConvertForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Convert XZP Version";
            this.Load += new System.EventHandler(this.ConvertForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCurrentVersion;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbVersion3;
        private System.Windows.Forms.RadioButton rbVersion1;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.Button btnCancel;
    }
}
