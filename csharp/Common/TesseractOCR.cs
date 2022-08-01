using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
                    Pix Pix = PixConverter.ToPix(ImageProcessing.ResizeImage(new Bitmap(Dummy), Dummy.Width * 2, Dummy.Height * 2));
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

        public string ProcessScreen(bool saveImage = false)
        {
            Rectangle Bounds = Screen.GetBounds(Point.Empty);

            using (Bitmap bitmap = new Bitmap(Bounds.Width, Bounds.Height))
            {
                using (Graphics graphic = Graphics.FromImage(bitmap))
                {
                    graphic.CopyFromScreen(Point.Empty, Point.Empty, Bounds.Size);
                }

                if (saveImage)
                {
                    bitmap.Save("screenCapture.bmp");
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

                Pix Pix = PixConverter.ToPix(ImageProcessing.ResizeImage(new Bitmap(bitmap), size.Width * 2, size.Height * 2));
                using (var page = Engine.Process(Pix))
                {
                    string text = page.GetText();
                    return text;
                }
            }
        }
    }
}