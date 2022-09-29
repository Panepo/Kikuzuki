using System;
using System.Windows;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;

namespace Kikuzuki
{
    public class FormatHelper
    {
        public static Bitmap BitmapImage2Bitmap(BitmapImage src)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(src));
                enc.Save(outStream);
                return new Bitmap(outStream);
            }
        }

        public static BitmapImage Bitmap2BitmapImage(Bitmap src)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                ((Bitmap)src).Save(memStream, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                memStream.Seek(0, System.IO.SeekOrigin.Begin);
                image.StreamSource = memStream;
                image.EndInit();
                return image;
            }
        }

        public static ImageSource Bitmap2ImageSource(Bitmap src)
        {
            IntPtr handle = src.GetHbitmap();
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public static Bitmap BitmapSource2Bitmap(BitmapSource src)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();

                enc.Frames.Add(BitmapFrame.Create(src));
                enc.Save(outStream);
                return new Bitmap(outStream);
            }
        }

        public static Bitmap ImageSource2Bitmap(ImageSource src)
        {
            var bmpFrame = BitmapFrame.Create((BitmapSource)src);

            var bmpEncoder = new BmpBitmapEncoder();
            bmpEncoder.Frames.Add(bmpFrame);

            var ms = new MemoryStream();
            bmpEncoder.Save(ms);

            return new Bitmap(ms);
        }
    }
}