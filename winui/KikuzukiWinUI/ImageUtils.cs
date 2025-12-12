using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Kikuzuki
{
    public static class ImageUtils
    {
        public static Bitmap DrawBoundingBox(Bitmap src, List<Rectangle> boxes)
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

        public static Bitmap DrawRectangleAndText(Bitmap src, List<Rectangle> boxes, string[] texts)
        {
            Bitmap dst = new Bitmap(src);
            using (Graphics graphic = Graphics.FromImage(dst))
            {
                Pen pen = new Pen(Color.Green, 2);
                boxes.ForEach(box =>
                {
                    graphic.DrawRectangle(pen, box);

                    int index = boxes.FindIndex(a => a.Contains(box));
                    graphic.DrawString(texts[index], new Font("Tahoma", 16), Brushes.Red, box);
                });

                return dst;
            }
        }

        public static Bitmap ReplaceRectangleAndText(Bitmap src, List<Rectangle> boxes, string[] texts)
        {
            Bitmap dst = new Bitmap(src);
            using (Graphics graphic = Graphics.FromImage(dst))
            {
                boxes.ForEach(box =>
                {
                    SolidBrush brush = new SolidBrush(src.GetPixel(box.X, box.Y));
                    graphic.FillRectangle(brush, box);

                    int index = boxes.FindIndex(a => a.Contains(box));
                    graphic.DrawString(texts[index], new Font("Tahoma", 16), Brushes.Red, box);
                });

                return dst;
            }
        }

        public static Bitmap ResizeImage(Bitmap src, int width, int height)
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

    public static class ImageFormatExtensions
    {
        public static ImageSource Bitmap2ImageSource(Bitmap src)
        {
            var wb = new WriteableBitmap(src.Width, src.Height);

            // Lock the bitmap's bits
            var rect = new Rectangle(0, 0, src.Width, src.Height);
            var bmpData = src.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                // Copy the pixel data directly
                using (var pixelStream = wb.PixelBuffer.AsStream())
                {
                    int bytes = Math.Abs(bmpData.Stride) * src.Height;
                    byte[] buffer = new byte[bytes];
                    System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, buffer, 0, bytes);
                    pixelStream.Write(buffer, 0, bytes);
                }
            }
            finally
            {
                src.UnlockBits(bmpData);
            }

            return wb;
        }

        public static Bitmap BitmapSource2Bitmap(BitmapSource src)
        {
            var wb = src as WriteableBitmap;
            if (wb == null)
                throw new ArgumentException("Only WriteableBitmap is supported.");

            using (var stream = wb.PixelBuffer.AsStream())
            {
                // Create a Bitmap from the pixel buffer
                var bmp = new Bitmap(wb.PixelWidth, wb.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var bmpData = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    bmp.PixelFormat);

                // Use Marshal.Copy instead of unsafe pointer code
                byte[] buffer = new byte[bmpData.Stride * bmpData.Height];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpData.Scan0, bytesRead);

                bmp.UnlockBits(bmpData);
                return bmp;
            }
        }
    }
}
