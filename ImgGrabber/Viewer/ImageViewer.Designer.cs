
namespace ImgGrabber
{
    partial class ImageViewer
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageViewer));
            this.panelImage = new System.Windows.Forms.Panel();
            this.buttonRoiAdd = new System.Windows.Forms.Button();
            this.buttonRoiRemove = new System.Windows.Forms.Button();
            this.buttonZoomIn = new System.Windows.Forms.Button();
            this.buttonZoomOut = new System.Windows.Forms.Button();
            this.panelImage.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelImage
            // 
            this.panelImage.Controls.Add(this.buttonRoiAdd);
            this.panelImage.Controls.Add(this.buttonRoiRemove);
            this.panelImage.Controls.Add(this.buttonZoomIn);
            this.panelImage.Controls.Add(this.buttonZoomOut);
            this.panelImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelImage.Location = new System.Drawing.Point(0, 0);
            this.panelImage.Name = "panelImage";
            this.panelImage.Size = new System.Drawing.Size(382, 809);
            this.panelImage.TabIndex = 2;
            // 
            // buttonRoiAdd
            // 
            this.buttonRoiAdd.Image = global::ImgGrabber.Properties.Resources.create;
            this.buttonRoiAdd.Location = new System.Drawing.Point(3, 3);
            this.buttonRoiAdd.Name = "buttonRoiAdd";
            this.buttonRoiAdd.Size = new System.Drawing.Size(35, 35);
            this.buttonRoiAdd.TabIndex = 7;
            this.buttonRoiAdd.UseVisualStyleBackColor = true;
            this.buttonRoiAdd.Visible = false;
            this.buttonRoiAdd.Click += new System.EventHandler(this.buttonRoiAdd_Click);
            // 
            // buttonRoiRemove
            // 
            this.buttonRoiRemove.Image = global::ImgGrabber.Properties.Resources.remove;
            this.buttonRoiRemove.Location = new System.Drawing.Point(40, 3);
            this.buttonRoiRemove.Name = "buttonRoiRemove";
            this.buttonRoiRemove.Size = new System.Drawing.Size(35, 35);
            this.buttonRoiRemove.TabIndex = 6;
            this.buttonRoiRemove.UseVisualStyleBackColor = true;
            this.buttonRoiRemove.Visible = false;
            this.buttonRoiRemove.Click += new System.EventHandler(this.buttonRoiRemove_Click);
            // 
            // buttonZoomIn
            // 
            this.buttonZoomIn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonZoomIn.Image = global::ImgGrabber.Properties.Resources.zoom_in;
            this.buttonZoomIn.Location = new System.Drawing.Point(307, 3);
            this.buttonZoomIn.Name = "buttonZoomIn";
            this.buttonZoomIn.Size = new System.Drawing.Size(35, 35);
            this.buttonZoomIn.TabIndex = 5;
            this.buttonZoomIn.UseVisualStyleBackColor = true;
            this.buttonZoomIn.Click += new System.EventHandler(this.buttonZoomIn_Click);
            // 
            // buttonZoomOut
            // 
            this.buttonZoomOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("buttonZoomOut.Image")));
            this.buttonZoomOut.Location = new System.Drawing.Point(344, 3);
            this.buttonZoomOut.Name = "buttonZoomOut";
            this.buttonZoomOut.Size = new System.Drawing.Size(35, 35);
            this.buttonZoomOut.TabIndex = 4;
            this.buttonZoomOut.UseVisualStyleBackColor = true;
            this.buttonZoomOut.Click += new System.EventHandler(this.buttonZoomOut_Click);
            // 
            // ImageViewer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.panelImage);
            this.Name = "ImageViewer";
            this.Size = new System.Drawing.Size(382, 809);
            this.panelImage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelImage;
        private System.Windows.Forms.Button buttonRoiAdd;
        private System.Windows.Forms.Button buttonRoiRemove;
        private System.Windows.Forms.Button buttonZoomIn;
        private System.Windows.Forms.Button buttonZoomOut;
    }
}
