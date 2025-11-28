using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImgGrabber
{
    public class TransMap
    {
        public static void TranferField(Matrix transferMatrix, float centerX, float centerY, float scaleX, float scaleY, float moveX, float moveY)
        {
            transferMatrix.Reset();
            transferMatrix.Scale(scaleX, -scaleY);
            transferMatrix.Translate(centerX + moveX * scaleX, centerY - moveY * scaleY, MatrixOrder.Append);
        }
        public static PointF GetWorldPoint(PointF point, Matrix transferMatrix) //point 를 TransformMatrix로 좌표 변환
        {
            PointF[] pointArray = { point };
            Matrix InvertTransformMatrix = transferMatrix.Clone();
            InvertTransformMatrix.Invert();
            InvertTransformMatrix.TransformPoints(pointArray);
            pointArray[0].X = (float)Math.Round(pointArray[0].X, 3);
            pointArray[0].Y = (float)Math.Round(pointArray[0].Y, 3);
            return pointArray[0];
        }
    }
}
