namespace TNU_AutoClass
{
    partial class FrmMain
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
            groupBox1 = new GroupBox();
            btnBD = new Button();
            btnClear = new Button();
            txtMK = new TextBox();
            txtTK = new TextBox();
            label2 = new Label();
            label1 = new Label();
            groupBox2 = new GroupBox();
            rtbTrangThai = new RichTextBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnBD);
            groupBox1.Controls.Add(btnClear);
            groupBox1.Controls.Add(txtMK);
            groupBox1.Controls.Add(txtTK);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(776, 100);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Cài đặt";
            // 
            // btnBD
            // 
            btnBD.Location = new Point(327, 28);
            btnBD.Name = "btnBD";
            btnBD.Size = new Size(106, 50);
            btnBD.TabIndex = 3;
            btnBD.Text = "Bắt đầu";
            btnBD.UseVisualStyleBackColor = true;
            btnBD.Click += btnBD_Click;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(439, 28);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(106, 50);
            btnClear.TabIndex = 4;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // txtMK
            // 
            txtMK.Location = new Point(69, 55);
            txtMK.Name = "txtMK";
            txtMK.PasswordChar = '*';
            txtMK.Size = new Size(240, 23);
            txtMK.TabIndex = 1;
            txtMK.TextChanged += txtMK_TextChanged;
            // 
            // txtTK
            // 
            txtTK.Location = new Point(69, 28);
            txtTK.Name = "txtTK";
            txtTK.Size = new Size(240, 23);
            txtTK.TabIndex = 0;
            txtTK.TextChanged += txtTK_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 58);
            label2.Name = "label2";
            label2.Size = new Size(57, 15);
            label2.TabIndex = 1;
            label2.Text = "Mật khẩu";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 31);
            label1.Name = "label1";
            label1.Size = new Size(57, 15);
            label1.TabIndex = 0;
            label1.Text = "Tài khoản";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(rtbTrangThai);
            groupBox2.Location = new Point(12, 118);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(776, 320);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Trạng thái";
            // 
            // rtbTrangThai
            // 
            rtbTrangThai.Location = new Point(6, 22);
            rtbTrangThai.Name = "rtbTrangThai";
            rtbTrangThai.Size = new Size(764, 292);
            rtbTrangThai.TabIndex = 5;
            rtbTrangThai.Text = "";
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "FrmMain";
            Text = "Menu chính";
            Load += FrmMain_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private TextBox txtMK;
        private TextBox txtTK;
        private Label label2;
        private Label label1;
        private Button btnBD;
        private Button btnClear;
        private GroupBox groupBox2;
        private RichTextBox rtbTrangThai;
    }
}