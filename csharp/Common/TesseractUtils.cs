using System.Drawing;

namespace Kikuzuki
{
    partial class TesseractOCR
    {
        private static Bitmap DrawBoundingBox(Bitmap src, System.Collections.Generic.List<Rectangle> boxes)
        {
            Bitmap dst = new Bitmap(src);
            using (Graphics graphic = Graphics.FromImage(dst))
            {
                Pen pen = new Pen(Color.Red, 2);
                boxes.ForEach(box =>
                {
                    graphic.DrawRectangle(pen, box);
                });

                return dst;
            }
        }
    }
}