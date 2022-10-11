using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace Kikuzuki
{
    partial class TesseractOCR
    {
        private static Bitmap DrawBoundingBox(Bitmap src, List<Rectangle> boxes)
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

        private static Bitmap ReplaceBoundingBox(Bitmap src, List<Rectangle> boxes, string[] texts)
        {
            Bitmap dst = new Bitmap(src);
            using (Graphics graphic = Graphics.FromImage(dst))
            {
                boxes.ForEach(box =>
                {
                    SolidBrush brush = new SolidBrush(src.GetPixel(box.X + 2, box.Y + 2));
                    graphic.FillRectangle(brush, box);

                    int index = boxes.FindIndex(a => a.Contains(box));
                    graphic.DrawString(texts[index], new Font("Tahoma", 16), Brushes.Black, box);
                });

                return dst;
            }
        }

        private static Bitmap ResizeImage(Bitmap src, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(src, destRect, 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}