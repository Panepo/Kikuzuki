using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace Kikuzuki
{
    public static class ImageUtils
    {
        public static Bitmap DrawBoundingBox(Bitmap src, List<Rectangle> boxes)
        {
            Bitmap dst = new(src);
            using Graphics graphic = Graphics.FromImage(dst);
            Pen pen = new(Color.Red, 2);
            boxes.ForEach(box =>
            {
                graphic.DrawRectangle(pen, box);
            });

            return dst;
        }

        public static Bitmap DrawRectangleAndText(Bitmap src, List<Rectangle> boxes, string[] texts)
        {
            Bitmap dst = new(src);
            using Graphics graphic = Graphics.FromImage(dst);
            Pen pen = new(Color.Green, 2);
            boxes.ForEach(box =>
            {
                graphic.DrawRectangle(pen, box);

                int index = boxes.FindIndex(a => a.Contains(box));
                graphic.DrawString(texts[index], new Font("Tahoma", 16), Brushes.Red, box);
            });

            return dst;
        }

        public static Bitmap ReplaceRectangleAndText(Bitmap src, List<Rectangle> boxes, string[] texts)
        {
            Bitmap dst = new(src);
            using Graphics graphic = Graphics.FromImage(dst);
            boxes.ForEach(box =>
            {
                SolidBrush brush = new(src.GetPixel(box.X, box.Y));
                graphic.FillRectangle(brush, box);

                int index = boxes.FindIndex(a => a.Contains(box));
                graphic.DrawString(texts[index], new Font("Tahoma", 16), Brushes.Red, box);
            });

            return dst;
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

                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(src, destRect, 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, wrapMode);
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
                using var pixelStream = wb.PixelBuffer.AsStream();
                int bytes = Math.Abs(bmpData.Stride) * src.Height;
                byte[] buffer = new byte[bytes];
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, buffer, 0, bytes);
                pixelStream.Write(buffer, 0, bytes);
            }
            finally
            {
                src.UnlockBits(bmpData);
            }

            return wb;
        }

        public static Bitmap BitmapSource2Bitmap(BitmapSource src)
        {
            var wb = src as WriteableBitmap ?? throw new ArgumentException("Only WriteableBitmap is supported.");
            using var stream = wb.PixelBuffer.AsStream();
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

        public static SoftwareBitmap BitmapSource2SoftwareBitmap(BitmapSource src)
        {
            var wb = src as WriteableBitmap ?? throw new ArgumentException("Only WriteableBitmap is supported.");
            int width = wb.PixelWidth;
            int height = wb.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];

            using (var stream = wb.PixelBuffer.AsStream())
            {
                stream.Read(pixels, 0, pixels.Length);
            }

            // Create SoftwareBitmap from BGRA8 buffer
            return SoftwareBitmap.CreateCopyFromBuffer(
                pixels.AsBuffer(),
                BitmapPixelFormat.Bgra8,
                width,
                height,
                BitmapAlphaMode.Premultiplied
            );
        }

        public static Bitmap SoftwareBitmapToBitmap(SoftwareBitmap softwareBitmap)
        {
            // Ensure format is BGRA8 and Premultiplied
            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            int width = softwareBitmap.PixelWidth;
            int height = softwareBitmap.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            softwareBitmap.CopyToBuffer(pixels.AsBuffer());

            // Create Bitmap and copy pixel data
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}
