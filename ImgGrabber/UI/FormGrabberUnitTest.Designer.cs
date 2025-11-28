
namespace ImgGrabber
{
    partial class FormGrabberUnitTest
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
            this.button_save = new System.Windows.Forms.Button();
            this.panel_Disp = new System.Windows.Forms.Panel();
            this.button_Exposure = new System.Windows.Forms.Button();
            this.button_Free = new System.Windows.Forms.Button();
            this.button_Stop = new System.Windows.Forms.Button();
            this.button_Grab = new System.Windows.Forms.Button();
            this.button_SingleGrab = new System.Windows.Forms.Button();
            this.button_Init = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_save
            // 
            this.button_save.Location = new System.Drawing.Point(581, 27);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(75, 41);
            this.button_save.TabIndex = 10;
            this.button_save.Text = "Save";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // panel_Disp
            // 
            this.panel_Disp.Location = new System.Drawing.Point(144, 76);
            this.panel_Disp.Name = "panel_Disp";
            this.panel_Disp.Size = new System.Drawing.Size(512, 347);
            this.panel_Disp.TabIndex = 9;
            // 
            // button_Exposure
            // 
            this.button_Exposure.Location = new System.Drawing.Point(491, 27);
            this.button_Exposure.Name = "button_Exposure";
            this.button_Exposure.Size = new System.Drawing.Size(84, 41);
            this.button_Exposure.TabIndex = 3;
            this.button_Exposure.Text = "Exposure Time";
            this.button_Exposure.UseVisualStyleBackColor = true;
            this.button_Exposure.Click += new System.EventHandler(this.button_Exposure_Click);
            // 
            // button_Free
            // 
            this.button_Free.Location = new System.Drawing.Point(420, 27);
            this.button_Free.Name = "button_Free";
            this.button_Free.Size = new System.Drawing.Size(66, 41);
            this.button_Free.TabIndex = 4;
            this.button_Free.Text = "Free";
            this.button_Free.UseVisualStyleBackColor = true;
            this.button_Free.Click += new System.EventHandler(this.button_Free_Click);
            // 
            // button_Stop
            // 
            this.button_Stop.Location = new System.Drawing.Point(355, 27);
            this.button_Stop.Name = "button_Stop";
            this.button_Stop.Size = new System.Drawing.Size(59, 41);
            this.button_Stop.TabIndex = 5;
            this.button_Stop.Text = "Stop";
            this.button_Stop.UseVisualStyleBackColor = true;
            this.button_Stop.Click += new System.EventHandler(this.button_Stop_Click);
            // 
            // button_Grab
            // 
            this.button_Grab.Location = new System.Drawing.Point(274, 27);
            this.button_Grab.Name = "button_Grab";
            this.button_Grab.Size = new System.Drawing.Size(75, 41);
            this.button_Grab.TabIndex = 6;
            this.button_Grab.Text = "Continous Grab";
            this.button_Grab.UseVisualStyleBackColor = true;
            this.button_Grab.Click += new System.EventHandler(this.button_Grab_Click);
            // 
            // button_SingleGrab
            // 
            this.button_SingleGrab.Location = new System.Drawing.Point(202, 27);
            this.button_SingleGrab.Name = "button_SingleGrab";
            this.button_SingleGrab.Size = new System.Drawing.Size(66, 41);
            this.button_SingleGrab.TabIndex = 7;
            this.button_SingleGrab.Text = "Single Grab";
            this.button_SingleGrab.UseVisualStyleBackColor = true;
            this.button_SingleGrab.Click += new System.EventHandler(this.button_SingleGrab_Click);
            // 
            // button_Init
            // 
            this.button_Init.Location = new System.Drawing.Point(144, 27);
            this.button_Init.Name = "button_Init";
            this.button_Init.Size = new System.Drawing.Size(52, 41);
            this.button_Init.TabIndex = 8;
            this.button_Init.Text = "Init";
            this.button_Init.UseVisualStyleBackColor = true;
            this.button_Init.Click += new System.EventHandler(this.button_Init_Click);
            // 
            // FormGrabberUnitTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.panel_Disp);
            this.Controls.Add(this.button_Exposure);
            this.Controls.Add(this.button_Free);
            this.Controls.Add(this.button_Stop);
            this.Controls.Add(this.button_Grab);
            this.Controls.Add(this.button_SingleGrab);
            this.Controls.Add(this.button_Init);
            this.Name = "FormGrabberUnitTest";
            this.Text = "FormGrabberUnitTest";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Panel panel_Disp;
        private System.Windows.Forms.Button button_Exposure;
        private System.Windows.Forms.Button button_Free;
        private System.Windows.Forms.Button button_Stop;
        private System.Windows.Forms.Button button_Grab;
        private System.Windows.Forms.Button button_SingleGrab;
        private System.Windows.Forms.Button button_Init;
    }
}