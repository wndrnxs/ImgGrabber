
namespace ImgGrabber
{
    partial class FormAuthority
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
            this.label15 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelOperator = new System.Windows.Forms.Label();
            this.labelMaintenance = new System.Windows.Forms.Label();
            this.labelEngineer = new System.Windows.Forms.Label();
            this.panelMain = new System.Windows.Forms.Panel();
            this.labelWrongPassword = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(13, 62);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(54, 20);
            this.label15.TabIndex = 23;
            this.label15.Text = "Level :";
            // 
            // buttonOK
            // 
            this.buttonOK.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOK.Location = new System.Drawing.Point(122, 157);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(89, 35);
            this.buttonOK.TabIndex = 25;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 20);
            this.label1.TabIndex = 26;
            this.label1.Text = "Password :";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxPassword.Location = new System.Drawing.Point(105, 107);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(277, 29);
            this.textBoxPassword.TabIndex = 28;
            this.textBoxPassword.Click += new System.EventHandler(this.textBoxPassword_Click);
            this.textBoxPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxPassword_KeyDown);
            // 
            // labelOperator
            // 
            this.labelOperator.BackColor = System.Drawing.Color.LightYellow;
            this.labelOperator.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelOperator.Font = new System.Drawing.Font("맑은 고딕", 9.75F);
            this.labelOperator.Location = new System.Drawing.Point(102, 56);
            this.labelOperator.Name = "labelOperator";
            this.labelOperator.Size = new System.Drawing.Size(90, 30);
            this.labelOperator.TabIndex = 29;
            this.labelOperator.Text = "Operator";
            this.labelOperator.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelOperator.Click += new System.EventHandler(this.labelOperator_Click);
            // 
            // labelMaintenance
            // 
            this.labelMaintenance.BackColor = System.Drawing.Color.LightGreen;
            this.labelMaintenance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelMaintenance.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMaintenance.Location = new System.Drawing.Point(197, 56);
            this.labelMaintenance.Name = "labelMaintenance";
            this.labelMaintenance.Size = new System.Drawing.Size(90, 30);
            this.labelMaintenance.TabIndex = 30;
            this.labelMaintenance.Text = "Maintenance";
            this.labelMaintenance.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelMaintenance.Click += new System.EventHandler(this.labelMaintenance_Click);
            // 
            // labelEngineer
            // 
            this.labelEngineer.BackColor = System.Drawing.Color.LightBlue;
            this.labelEngineer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelEngineer.Font = new System.Drawing.Font("맑은 고딕", 9.75F);
            this.labelEngineer.Location = new System.Drawing.Point(292, 56);
            this.labelEngineer.Name = "labelEngineer";
            this.labelEngineer.Size = new System.Drawing.Size(90, 30);
            this.labelEngineer.TabIndex = 31;
            this.labelEngineer.Text = "Engineer";
            this.labelEngineer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelEngineer.Click += new System.EventHandler(this.labelEngineer_Click);
            // 
            // panelMain
            // 
            this.panelMain.BackColor = System.Drawing.Color.White;
            this.panelMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMain.Controls.Add(this.labelWrongPassword);
            this.panelMain.Controls.Add(this.buttonCancel);
            this.panelMain.Controls.Add(this.label3);
            this.panelMain.Controls.Add(this.label2);
            this.panelMain.Controls.Add(this.labelEngineer);
            this.panelMain.Controls.Add(this.labelMaintenance);
            this.panelMain.Controls.Add(this.labelOperator);
            this.panelMain.Controls.Add(this.textBoxPassword);
            this.panelMain.Controls.Add(this.label1);
            this.panelMain.Controls.Add(this.buttonOK);
            this.panelMain.Controls.Add(this.label15);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(425, 203);
            this.panelMain.TabIndex = 32;
            // 
            // labelWrongPassword
            // 
            this.labelWrongPassword.AutoSize = true;
            this.labelWrongPassword.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWrongPassword.Location = new System.Drawing.Point(165, 139);
            this.labelWrongPassword.Name = "labelWrongPassword";
            this.labelWrongPassword.Size = new System.Drawing.Size(116, 15);
            this.labelWrongPassword.TabIndex = 35;
            this.labelWrongPassword.Text = "Wrong password....!";
            this.labelWrongPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelWrongPassword.Visible = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(217, 157);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(89, 35);
            this.buttonCancel.TabIndex = 34;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("맑은 고딕", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(165, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 25);
            this.label3.TabIndex = 33;
            this.label3.Text = "Authority";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(165, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 17);
            this.label2.TabIndex = 32;
            // 
            // FormAuthority
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(425, 203);
            this.Controls.Add(this.panelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormAuthority";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FormAuthority";
            this.Load += new System.EventHandler(this.FormAuthority_Load);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelOperator;
        private System.Windows.Forms.Label labelMaintenance;
        private System.Windows.Forms.Label labelEngineer;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelWrongPassword;
    }
}