using System;
using System.Windows;

namespace Kikuzuki
{
    internal class FormatHelper
    {
        public static System.Drawing.Bitmap BitmapImage2Bitmap(System.Windows.Media.Imaging.BitmapImage src)
        {
            using (System.IO.MemoryStream outStream = new System.IO.MemoryStream())
            {
                System.Windows.Media.Imaging.BitmapEncoder enc = new System.Windows.Media.Imaging.BmpBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(src));
                enc.Save(outStream);
                return new System.Drawing.Bitmap(outStream);
            }
        }

        public static System.Windows.Media.Imaging.BitmapImage Bitmap2BitmapImage(System.Drawing.Bitmap src)
        {
            using (System.IO.MemoryStream memStream = new System.IO.MemoryStream())
            {
                ((System.Drawing.Bitmap)src).Save(memStream, System.Drawing.Imaging.ImageFormat.Bmp);
                System.Windows.Media.Imaging.BitmapImage image = new System.Windows.Media.Imaging.BitmapImage();
                image.BeginInit();
                memStream.Seek(0, System.IO.SeekOrigin.Begin);
                image.StreamSource = memStream;
                image.EndInit();
                return image;
            }
        }

        public static System.Windows.Media.ImageSource Bitmap2ImageSource(System.Drawing.Bitmap src)
        {
            IntPtr handle = src.GetHbitmap();
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        public static System.Drawing.Bitmap BitmapSource2Bitmap(System.Windows.Media.Imaging.BitmapSource src)
        {
            using (System.IO.MemoryStream outStream = new System.IO.MemoryStream())
            {
                System.Windows.Media.Imaging.BitmapEncoder enc = new System.Windows.Media.Imaging.BmpBitmapEncoder();

                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(src));
                enc.Save(outStream);
                return new System.Drawing.Bitmap(outStream);
            }
        }
    }
}