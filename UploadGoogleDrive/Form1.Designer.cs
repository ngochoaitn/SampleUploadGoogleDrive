namespace UploadGoogleDrive
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnKetNoiTaiKhoanGoogle = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDuongDanTep = new System.Windows.Forms.TextBox();
            this.btnChonTep = new System.Windows.Forms.Button();
            this.btnTaiLen = new System.Windows.Forms.Button();
            this.txtNhatKy = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnKetNoiTaiKhoanGoogle
            // 
            this.btnKetNoiTaiKhoanGoogle.Location = new System.Drawing.Point(125, 12);
            this.btnKetNoiTaiKhoanGoogle.Name = "btnKetNoiTaiKhoanGoogle";
            this.btnKetNoiTaiKhoanGoogle.Size = new System.Drawing.Size(145, 23);
            this.btnKetNoiTaiKhoanGoogle.TabIndex = 0;
            this.btnKetNoiTaiKhoanGoogle.Text = "Kết nối Google Drive";
            this.btnKetNoiTaiKhoanGoogle.UseVisualStyleBackColor = true;
            this.btnKetNoiTaiKhoanGoogle.Click += new System.EventHandler(this.btnKetNoiTaiKhoanGoogle_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Đường dẫn tệp tải lên:";
            // 
            // txtDuongDanTep
            // 
            this.txtDuongDanTep.Location = new System.Drawing.Point(125, 41);
            this.txtDuongDanTep.Name = "txtDuongDanTep";
            this.txtDuongDanTep.Size = new System.Drawing.Size(228, 20);
            this.txtDuongDanTep.TabIndex = 2;
            // 
            // btnChonTep
            // 
            this.btnChonTep.Location = new System.Drawing.Point(359, 39);
            this.btnChonTep.Name = "btnChonTep";
            this.btnChonTep.Size = new System.Drawing.Size(75, 23);
            this.btnChonTep.TabIndex = 3;
            this.btnChonTep.Text = "Chọn tệp";
            this.btnChonTep.UseVisualStyleBackColor = true;
            this.btnChonTep.Click += new System.EventHandler(this.btnChonTep_Click);
            // 
            // btnTaiLen
            // 
            this.btnTaiLen.Location = new System.Drawing.Point(125, 67);
            this.btnTaiLen.Name = "btnTaiLen";
            this.btnTaiLen.Size = new System.Drawing.Size(75, 23);
            this.btnTaiLen.TabIndex = 4;
            this.btnTaiLen.Text = "Tải lên";
            this.btnTaiLen.UseVisualStyleBackColor = true;
            this.btnTaiLen.Click += new System.EventHandler(this.btnTaiLen_Click);
            // 
            // txtNhatKy
            // 
            this.txtNhatKy.Location = new System.Drawing.Point(10, 116);
            this.txtNhatKy.Multiline = true;
            this.txtNhatKy.Name = "txtNhatKy";
            this.txtNhatKy.ReadOnly = true;
            this.txtNhatKy.Size = new System.Drawing.Size(424, 132);
            this.txtNhatKy.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 260);
            this.Controls.Add(this.txtNhatKy);
            this.Controls.Add(this.btnTaiLen);
            this.Controls.Add(this.btnChonTep);
            this.Controls.Add(this.txtDuongDanTep);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnKetNoiTaiKhoanGoogle);
            this.Name = "Form1";
            this.Text = "Ví dụ tải tệp lên Google Drive";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnKetNoiTaiKhoanGoogle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDuongDanTep;
        private System.Windows.Forms.Button btnChonTep;
        private System.Windows.Forms.Button btnTaiLen;
        private System.Windows.Forms.TextBox txtNhatKy;
    }
}

