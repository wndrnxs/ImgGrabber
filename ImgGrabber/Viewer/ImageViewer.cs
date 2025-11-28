using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImgGrabber
{
    public delegate void DelegateSelectedROI(ROI roi);
    public delegate void DelegateCreateROI();
    public delegate void DelegateRemoveROI();
    public partial class ImageViewer : UserControl
    {

        public event DelegateSelectedROI EventSelectedROI;
        public event DelegateCreateROI EventCreateROI;
        public event DelegateRemoveROI EventRemoveROI;

        private readonly Panel panel;
        /// <summary>
        /// Source Image와 Panel의 비율: Panel / Image * 100d
        /// </summary>
        private double ratio;
        private Point moveClickPoint;
        private Point offset;
        private Size imageSize;
        private bool useRoiEdit;

        public Panel Viewer => panel;
        public void UseRoiEdit(bool value)
        {
            buttonRoiAdd.Visible = value;
            buttonRoiRemove.Visible = value;
            useRoiEdit = value;
        }
        public Size ImageSize => imageSize;

        public ListROI ListRoi
        {
            get => listRoi;
            set
            {
                listRoi = value;
                panel.Invalidate();
            }
        }

        private int imageIndex;
        private string imageName;
        private int imageWidth;
        private int imageHeight;

        private ListROI listRoi;

        private ROI drawRoi;

        public ImageViewer()
        {
            InitializeComponent();

            Dock = DockStyle.Fill;

            panel = new Panel
            {
                Parent = panelImage,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.MouseWheel += ImageViewer_MouseWheel;
            panel.MouseMove += ImageViewer_MouseMove;
            panel.MouseDown += ImageViewer_MouseDown;
            panel.MouseUp += Panel_MouseUp;
            panel.Paint += Panel_Paint;
            panel.MouseEnter += Panel_MouseEnter;
            panel.PreviewKeyDown += Panel_PreviewKeyDown;

            panelImage.MouseWheel += ImageViewer_MouseWheel;
            panelImage.MouseDown += ImageViewer_MouseDown;
            panelImage.Resize += ImageViewer_Resize;
        }

        private void Panel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if(useRoiEdit)
            {
                if (drawRoi != null)
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        drawRoi.Clear();
                        drawRoi = null;
                        panelImage.Cursor = Cursors.Arrow;
                        panel.Invalidate();
                    }
                }
                else
                {
                    if (e.KeyCode == Keys.Delete)
                    {
                        RemoveROI();
                    }
                }
            }
        }

        private void Panel_MouseEnter(object sender, EventArgs e)
        {
            panel.Focus();
        }

        private void ImageViewer_Resize(object sender, EventArgs e)
        {
            ResetSize();
        }

        public void AddRoi(ROI roi)
        {
            if(listRoi == null)
            {
                listRoi = new ListROI();
            }

            listRoi.Add(roi);
            panel.Invalidate();
        }

        public void ClearRois()
        {
            listRoi.Clear();
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            using (Graphics g = e.Graphics)
            {
                if (ratio > 0)
                {
                    g.ScaleTransform((float)ratio, (float)ratio);
                }

                if (useRoiEdit)
                {
                    if (drawRoi != null)
                    {
                        using (Pen pen = new Pen(Color.Black))
                        {
                            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                            drawRoi.Draw(g, pen);
                        }
                    }
                }
                else
                {
                    using (Font font = new Font("맑은고딕", (float)(10d / ratio), FontStyle.Regular))
                    {
                        g.DrawString($"Index:{imageIndex}\nName:{imageName}\nWidth:{imageWidth}\nHeight:{imageHeight}", font, Brushes.Red, new Point(0, 0));
                    }
                }


                if (listRoi != null)
                {
                    using (Pen pen = new Pen(Color.Red))
                    {
                        foreach (ROI roi in listRoi)
                        {
                            roi.Draw(g, pen);

                            if (roi.Selected)
                            {
                                using (Pen SelectedPen = new Pen(Color.Gold, 0.05f))
                                {
                                    g.DrawRectangle(SelectedPen, roi.GetBound());
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void ReDraw()
        {
            panel.Invalidate();
        }

        public void SetSize(Size imageSize)
        {
            this.imageSize = imageSize;
            ResetSize();
        }

        public void ResetSize()
        {
            FitRatio();

            offset = new Point(0, 0);

            panel.Invalidate();
        }

        private void Transform()
        {
            if (panel.InvokeRequired)
            {
                panel.Invoke(new Action(() =>
                {
                    panel.Width = (int)Math.Round(imageSize.Width * ratio);
                    panel.Height = (int)Math.Round(imageSize.Height * ratio);

                    panel.Location = new Point(
                            (int)(panelImage.Width / 2f - panel.Width / 2f + offset.X),
                            (int)(panelImage.Height / 2f - panel.Height / 2f + offset.Y));
                }));
            }
            else
            {
                panel.Width = (int)Math.Round(imageSize.Width * ratio);
                panel.Height = (int)Math.Round(imageSize.Height * ratio);

                panel.Location = new Point(
                        (int)(panelImage.Width / 2f - panel.Width / 2f + offset.X),
                        (int)(panelImage.Height / 2f - panel.Height / 2f + offset.Y));
            }

            panel.Invalidate();
        }


        private void FitRatio()
        {
            double ScaleWH;
            double compareChildXYScale = imageSize.Height / (double)imageSize.Width;
            double comparepanelXYScale = panelImage.Height / (double)panelImage.Width;

            if (compareChildXYScale > comparepanelXYScale) //panel의 가로가 더 큼
            {
                ScaleWH = imageSize.Width / (double)imageSize.Height;
                if (panel.InvokeRequired)
                {
                    panel.Invoke(new Action(() =>
                    {
                        panel.Height = panelImage.Height;
                        panel.Width = (int)(panelImage.Height * ScaleWH);
                        panel.Location = new Point((int)(panelImage.Width / 2f - panel.Width / 2f), 0);
                    }));
                }
                else
                {
                    panel.Height = panelImage.Height;
                    panel.Width = (int)(panelImage.Height * ScaleWH);
                    panel.Location = new Point((int)(panelImage.Width / 2f - panel.Width / 2f), 0);
                }

                if (panel.Height > 0)
                {
                    ratio = panel.Height / (double)imageSize.Height;
                }
            }
            else
            {
                ScaleWH = imageSize.Height / (double)imageSize.Width;
                if (panel.InvokeRequired)
                {
                    panel.Invoke(new Action(() =>
                    {
                        panel.Width = panelImage.Width;
                        panel.Height = (int)(panelImage.Width * ScaleWH);
                        panel.Location = new Point(0, (int)(Height / 2f - panel.Height / 2f));
                    }));
                }
                else
                {
                    panel.Width = panelImage.Width;
                    panel.Height = (int)(panelImage.Width * ScaleWH);
                    panel.Location = new Point(0, (int)(Height / 2f - panel.Height / 2f));
                }

                if (panel.Width > 0)
                {
                    ratio = panel.Width / (double)imageSize.Width;
                }
            }
        }

        private void ImageViewer_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        Point p = GetPoint(e.Location);

                        if (useRoiEdit)
                        {
                            if (drawRoi != null)
                            {
                                if (drawRoi.VertexCount < drawRoi.NeedVertexCount)
                                {
                                    drawRoi.AddVertex(p);
                                    drawRoi.DrawingVertex(p);
                                }
                                else
                                {
                                    if (drawRoi.GetBound().Contains(p))
                                    {
                                        listRoi.Add(drawRoi.Clone());
                                    }

                                    drawRoi.Clear();
                                    drawRoi = null;
                                    panelImage.Cursor = Cursors.Arrow;
                                    EventCreateROI?.Invoke();
                                }
                            }
                            else
                            {
                                ROI roi = listRoi.ContainRoi(p);
                                if (roi != null)
                                {
                                    listRoi.AllSelect(false);
                                    roi.Selected = true;
                                    buttonRoiRemove.Enabled = true;
                                }
                                else
                                {
                                    moveClickPoint = e.Location;
                                    listRoi.AllSelect(false);
                                    buttonRoiRemove.Enabled = false;
                                }


                                EventSelectedROI?.Invoke(roi);
                            }
                        }
                        else
                        {
                            moveClickPoint = e.Location;
                        }


                        panel.Invalidate();
                    }
                    break;

                case MouseButtons.Right:
                    {
                        ResetSize();
                    }
                    break;
            }
        }

        internal void SetInfo(int index, string name, Size size)
        {
            imageIndex = index;
            imageName = name;
            imageWidth = size.Width;
            imageHeight = size.Height;
        }

        private Point GetPoint(Point location)
        {
            return new Point((int)(location.X / ratio), (int)(location.Y / ratio));
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        if (!moveClickPoint.IsEmpty)
                        {
                            moveClickPoint = new Point();
                        }
                    }
                    break;
            }
        }

        private void ImageViewer_MouseMove(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        if (!moveClickPoint.IsEmpty)
                        {
                            offset.X += (int)((e.Location.X - moveClickPoint.X) * ratio);
                            offset.Y += (int)((e.Location.Y - moveClickPoint.Y) * ratio);

                            Transform();
                        }
                    }
                    break;
                case MouseButtons.None:
                    {
                        if (useRoiEdit)
                        {
                            if (drawRoi != null)
                            {
                                drawRoi.DrawingVertex(GetPoint(e.Location));
                                panel.Invalidate();
                            }
                        }

                    }
                    break;
            }
        }

        private void ImageViewer_MouseWheel(object sender, MouseEventArgs e)
        {
            int lines = e.Delta * SystemInformation.MouseWheelScrollLines / 120;

            if (lines > 0)
            {
                ZoomIn();
            }
            else
            {
                ZoomOut();
            }

            Transform();

        }

        private void ZoomOut()
        {  
            ratio *= 0.9d;
            if (ratio < 0.01d)
            {
                ratio = 0.01d;
            } 
            Transform();
        }

        private void ZoomIn()
        {
            ratio *= 1.1d;
            if (ratio > 2d)
            {
                ratio = 2d;
            }
            Transform();
        }

        internal void RemoveRoi(ROI roi)
        {
            if (listRoi.Contains(roi))
            {
                listRoi.Remove(roi);
            }

            panel.Invalidate();
        }

        internal void AddRois(ListROI rois)
        {
            foreach (ROI roi in rois)
            {
                listRoi.Add(roi);
            }

            panel.Invalidate();
        }

        internal bool ContainsRoi(ROI roi)
        {
            return listRoi.Contains(roi);
        }

        private void buttonRoiAdd_Click(object sender, EventArgs e)
        {
            drawRoi = new ROI(listRoi);
            panelImage.Cursor = Cursors.Cross;
            listRoi.AllSelect(false);
        }

        private void buttonRoiRemove_Click(object sender, EventArgs e)
        {
            RemoveROI();
        }

        private void RemoveROI()
        {
            ROI Roi = listRoi.SelectedRoi();
            if (Roi != null)
            {
                listRoi.Remove(Roi);
                panel.Invalidate();
                EventRemoveROI?.Invoke();
            }
        }

        private void buttonZoomIn_Click(object sender, EventArgs e)
        {
            ZoomIn();
        }

        private void buttonZoomOut_Click(object sender, EventArgs e)
        {
            ZoomOut();
        }
    }
}
