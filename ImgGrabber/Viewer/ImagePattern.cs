using System.Drawing;

namespace ImgGrabber
{
    public class ImagePattern
    {
        private readonly Image image;
        private Point position;
        public ImagePattern(Image image, Point position)
        {
            this.image = image;
            this.position = position;
        }

        public Image Image => image;

        internal void Draw(Graphics g, Pen pen)
        {
            g.DrawImage(image, position);
        }
    }
}
