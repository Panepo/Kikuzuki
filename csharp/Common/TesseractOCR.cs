using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;

namespace Kikuzuki
{
    class TesseractOCR
    {
        private readonly TesseractEngine Engine;

        public TesseractOCR(string dataPath, string language)
        {
            Engine = new TesseractEngine(dataPath, language);
        }

        public string ProcessFile(string imagePath)
        {
            try
            {
                using (Image Dummy = Image.FromFile(imagePath))
                {
                    Pix Pix = PixConverter.ToPix(new Bitmap(Dummy));
                    using (var page = Engine.Process(Pix))
                    {
                        string text = page.GetText();
                        return text;
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(@"File not found.", e);
            }
        }

        public string ProcessScreen()
        {
            Rectangle Bounds = Screen.GetBounds(Point.Empty);

            using (Bitmap bitmap = new Bitmap(Bounds.Width, Bounds.Height))
            {
                using (Graphics graphic = Graphics.FromImage(bitmap))
                {
                    graphic.CopyFromScreen(Point.Empty, Point.Empty, Bounds.Size);
                }

                Pix Pix = PixConverter.ToPix(new Bitmap(bitmap));
                using (var page = Engine.Process(Pix))
                {
                    string text = page.GetText();
                    return text;
                }
            }
        }

        public string ProcessScreen(Point x, Point y, Size size, bool saveImage = false)
        {
            using (Bitmap bitmap = new Bitmap(size.Width, size.Height))
            {
                using (Graphics graphic = Graphics.FromImage(bitmap))
                {
                    graphic.CopyFromScreen(x, y, size);
                }

                if (saveImage)
                {
                    bitmap.Save("screenCapture.bmp");
                }

                Pix Pix = PixConverter.ToPix(ResizeImage(new Bitmap(bitmap), size.Width*2, size.Height*2));
                using (var page = Engine.Process(Pix))
                {
                    string text = page.GetText();
                    return text;
                }
            }
        }

        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

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
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}