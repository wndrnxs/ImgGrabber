using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace ImgGrabber
{
    public class ROI
    {
        [Category("\tImage"), DisplayName("Name")]
        public string Name { get; set; }
        [Category("\tImage"), DisplayName("Cell No(1~)")]
        public int CellNo
        {
            get => cellNo;
            set
            {
                cellNo = value;

                if (cellNo <= 0)
                {
                    cellNo = 1;
                }
            }
        }
        [Category("\tImage"), DisplayName("\tIndex(1~)"), ReadOnly(true)]
        public int Index
        {
            get
            {
                if (parent != null)
                {
                    return parent.IndexOf(this) + 1;
                }
                else
                {
                    return 0;
                }
            }
        }
        [Category("Area"), DisplayName("Size")]
        public Size Size
        {
            get => rectangle.Size;
            set => rectangle.Size = value;
        }
        [Category("Area"), DisplayName("Offset")]
        public Point Location
        {
            get => rectangle.Location;
            set => rectangle.Location = value;
        }

        internal int X
        {
            get => rectangle.X;
            set => rectangle.X = value;
        }
        internal int Y
        {
            get => rectangle.Y;
            set => rectangle.Y = value;
        }
        internal int Width
        {
            get => rectangle.Width;
            set => rectangle.Width = value;
        }
        internal int Height
        {
            get => rectangle.Height;
            set => rectangle.Height = value;
        }
        internal int VertexCount => drawNeedPoints.Count;
        internal bool Selected { get; set; }
        internal int cellNo = 1;
        internal int NeedVertexCount => 2;

        private readonly List<Point> drawNeedPoints = new List<Point>();
        private ListROI parent;
        private Rectangle rectangle;

        public ROI(ListROI parent)
        {
            this.parent = parent;
            rectangle = new Rectangle(0,0,100,100);
        }
        public ROI(ListROI parent, Rectangle rectangle) : this(parent)
        {
            this.rectangle = rectangle;
        }

        internal ROI Clone()
        {
            var roi = new ROI(parent, new Rectangle(rectangle.Location, rectangle.Size));
            return roi;
        }

        internal void Draw(Graphics g, Pen pen)
        {
            if (!rectangle.Size.IsEmpty)
            {
                g.DrawRectangle(pen, rectangle);
                using (Brush brush = new SolidBrush(Color.FromArgb(10, 255, 0, 0)))
                {
                    g.FillRectangle(brush, rectangle);
                }
            }
        }

        internal Rectangle GetBound()
        {
            return new Rectangle(rectangle.Location, rectangle.Size);
        }

        internal void Clear()
        {
            rectangle = new Rectangle();
        }

        internal void AddVertex(Point point)
        {
            drawNeedPoints.Add(point);
        }

        internal void DrawingVertex(Point nextPoint)
        {
            if (drawNeedPoints.Count == 1) // 1 Step Draw
            {
                Point StartPoint = drawNeedPoints[0];
                int Width = Math.Abs(drawNeedPoints[0].X - nextPoint.X);
                int Height = Math.Abs(drawNeedPoints[0].Y - nextPoint.Y);
                if (drawNeedPoints[0].X > nextPoint.X)
                {
                    if (drawNeedPoints[0].Y > nextPoint.Y)
                    {
                        rectangle.Location = nextPoint;
                    }
                    else
                    {
                        rectangle.Location = new Point(nextPoint.X, StartPoint.Y);
                    }
                }
                else
                {
                    if (drawNeedPoints[0].Y > nextPoint.Y)
                    {
                        rectangle.Location = new Point(StartPoint.X, nextPoint.Y);
                    }
                    else
                    {
                        rectangle.Location = StartPoint;
                    }
                }
                rectangle.Size = new Size(Width, Height);
            }
        }
    }

    public class ListROI : List<ROI>
    {
        public ListROI()
        {

        }

        internal bool Contain(Point location)
        {
            foreach (ROI ptn in this)
            {
                if (ptn.GetBound().Contains(location))
                {
                    return true;
                }
            }

            return false;
        }

        internal ROI ContainRoi(Point location)
        {
            foreach (ROI ptn in this)
            {
                if (ptn.GetBound().Contains(location))
                {
                    return ptn;
                }
            }

            return null;
        }

        internal void AllSelect(bool v)
        {
            foreach (ROI ptn in this)
            {
                ptn.Selected = v;
            }
        }

        internal ROI SelectedRoi()
        {
            foreach (ROI ptn in this)
            {
                if(ptn.Selected)
                {
                    return ptn;
                }
            }

            return null;
        }
    }
}