using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ImgGrabber
{
    public partial class CustomImageView : UserControl
    {
        private float initScale = 1f;
        private Image image;
        private readonly Matrix transformMatrix = new Matrix();
        private float scale;
        private PointF center;
        private PointF offset = new PointF(0, 0);
        private PointF beforeCenter;
        private bool flagPaintEvent;
        private Panel panel = new Panel();
        private Bitmap BackImage;
        private bool enable;

        public Image Image
        {
            get => image;
            set
            {
                if (value != null)
                {
                    if (image != null)
                    {
                        image.Dispose();
                        image = null;
                    }

                    image = (Bitmap)new Bitmap(value).Clone();
                    image.RotateFlip(RotateFlipType.Rotate180FlipX);
                    UpdateImage();
                    ReDrawImage();
                }
            }
        }

        public Panel Panel { get => panel; set => panel = value; }
        public bool Enable { get => enable; set => enable = value; }
        public float InitScale { get => initScale; set => initScale = value; }

        public float GetScale()
        {
            return scale;
        }

        public CustomImageView()
        {
            InitializeComponent();

            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer |ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true );
            //this.UpdateStyles();

            pictureBoxImage.MouseWheel += PictureBoxImage_MouseWheel;
            scale = initScale;

            //center = new PointF(Width / 2, Height / 2);
        }

        internal void UpdatePanelImage()
        {
            UpdateImage();
            UpdateTranferField();
            ReDrawImage();
        }

        private void pictureBoxImage_Resize(object sender, EventArgs e)
        {
            center = new PointF(Width / 2, Height / 2);

            UpdateImage();
            UpdateTranferField();
            ReDrawImage();
        }

        private void pictureBoxImage_Paint(object sender, PaintEventArgs e)
        {
            if (!enable)
            {
                return;
            }

            if (flagPaintEvent)
            {
                UpdateImage();
                pictureBoxImage.Image = BackImage;
                flagPaintEvent = false;
            }
        }

        private void pictureBoxImage_MouseEnter(object sender, EventArgs e)
        {
            Focus();
        }

        private void pictureBoxImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (!enable)
            {
                return;
            }

            PointF CurrentPoint = TransMap.GetWorldPoint(e.Location, transformMatrix);
            ImageMouseDown(e, CurrentPoint);
            ReDrawImage();
        }
        private void pictureBoxImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!enable)
            {
                return;
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        PointF CurrentPoint = TransMap.GetWorldPoint(e.Location, transformMatrix);
                        MapTransform(new PointF(CurrentPoint.X - beforeCenter.X, CurrentPoint.Y - beforeCenter.Y));
                    }
                    break;
            }
        }
        private void PictureBoxImage_MouseWheel(object sender, MouseEventArgs e)
        {
            ImageMouseWheel(e);
            ReDrawImage();
        }


        private void UpdateTranferField()
        {
            TransMap.TranferField(transformMatrix, center.X, center.Y, scale, scale, offset.X, offset.Y);
        }

        private void UpdateImage()
        {
            try
            {
                if (Width > 0 && Height > 0)
                {
                    BackImage = new Bitmap(Width, Height);

                    using (Graphics gr = Graphics.FromImage(BackImage))
                    {
                        if (image != null)
                        {
                            gr.Transform = transformMatrix;
                            //gr.DrawImage(image, new PointF(0, 0));

                            //gr.Clear(Color.Black);
                            //RectangleF dstRect = new RectangleF(-initWidth / 2, -initHeight / 2, initWidth, initHeight);
                            RectangleF dstRect = new RectangleF(-image.Width / 2, -image.Height / 2, image.Width, image.Height);
                            RectangleF srcRect = new RectangleF(0, 0, image.Width, image.Height);
                            gr.DrawImage(image, dstRect, srcRect, GraphicsUnit.Pixel);
                        }
                    }
                }

            }
            catch (Exception)
            {
                //MessageBox.Show(E.Message);
            }

        }

        private void ReDrawImage()
        {
            flagPaintEvent = true;
            pictureBoxImage.Invalidate();
        }

        private void ImageMouseDown(MouseEventArgs e, PointF currentPoint)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        beforeCenter = currentPoint;
                    }
                    break;

                case MouseButtons.Right:
                    {
                        MapReset();
                    }
                    break;
            }
        }


        internal void MapTransform(PointF pointF)
        {
            offset.X += pointF.X;
            offset.Y += pointF.Y;

            UpdateTranferField();
            ReDrawImage();
        }

        internal void MapReset()
        {
            scale = initScale;
            offset = new PointF(0.0f, 0.0f);
            UpdateTranferField();
            ReDrawImage();
        }


        private void ImageMouseWheel(MouseEventArgs e)
        {
            if (!enable)
            {
                return;
            }

            if (0 < e.Delta)
            {
                ZoomIn();
            }
            else
            {
                ZoomOut();
            }

            //UpdateTranferField();
        }

        private void ZoomIn()
        {
            float scale;
            if (this.scale >= 10f)
            {
                scale = this.scale + 1f;
            }
            else if (this.scale >= 1f)
            {
                scale = this.scale + 0.1f;
            }
            else if (this.scale >= 0.1f)
            {
                scale = this.scale + 0.01f;
            }
            else if (this.scale >= 0.01f)
            {
                scale = this.scale + 0.001f;
            }
            else if (this.scale >= 0.001f)
            {
                scale = this.scale + 0.0001f;
            }
            else
            {
                scale = this.scale + 0.00001f;
            }

            if (this.scale > 100f)
            {
                return;
            }

            this.scale = scale;
            UpdateTranferField();
        }

        private void ZoomOut()
        {
            float scale;

            if (this.scale > 10f)
            {
                scale = this.scale - 1f;
            }
            else if (this.scale > 1f)
            {
                scale = this.scale - 0.1f;
            }
            else if (this.scale > 0.1f)
            {
                scale = this.scale - 0.01f;
            }
            else if (this.scale > 0.01f)
            {
                scale = this.scale - 0.001f;
            }
            else if (this.scale > 0.001f)
            {
                scale = this.scale - 0.0001f;
            }
            else
            {
                return;
            }

            this.scale = scale;
            UpdateTranferField();
        }
    }
}
