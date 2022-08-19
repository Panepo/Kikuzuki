using System;
using System.IO;
using System.Windows;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Interop;

namespace Kikuzuki
{
    internal class FormatHelper
    {
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static BitmapImage Bitmap2BitmapImage(Bitmap src)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                ((System.Drawing.Bitmap)src).Save(memStream, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                memStream.Seek(0, SeekOrigin.Begin);
                image.StreamSource = memStream;
                image.EndInit();
                return image;
            }
        }

        public static ImageSource Bitmap2ImageSource(Bitmap bitmap)
        {
            IntPtr handle = bitmap.GetHbitmap();
            return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}