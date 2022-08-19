using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;
using System.IO;
using Tesseract;

namespace Kikuzuki
{
    internal class TesseractOCR
    {
        private static readonly string OCRLang = "eng";
        private static readonly string OCRPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly EngineMode OCRMode = EngineMode.TesseractAndLstm;

        private static Mat ImageEnhance(Mat src)
        {
            Mat dst = new Mat();

            if (src.Width > src.Height)
            {
                if (src.Width <= 300)
                {
                    Mat resized = new Mat();
                    OpenCvSharp.Size size = new OpenCvSharp.Size(src.Width * 2, src.Height * 2);
                    Cv2.Resize(src, resized, size, 0, 0, InterpolationFlags.Cubic);

                    Cv2.Threshold(resized, dst, 127, 255, ThresholdTypes.Binary);
                }
                else
                {
                    Cv2.Threshold(src, dst, 127, 255, ThresholdTypes.Binary);
                }
            }
            else
            {
                if (src.Height <= 300)
                {
                    Mat resized = new Mat();
                    OpenCvSharp.Size size = new OpenCvSharp.Size(src.Width * 2, src.Height * 2);
                    Cv2.Resize(src, resized, size, 0, 0, InterpolationFlags.Cubic);
                    Cv2.Threshold(resized, dst, 127, 255, ThresholdTypes.Binary);
                }
                else
                {
                    Cv2.Threshold(src, dst, 127, 255, ThresholdTypes.Binary);
                }
            }

            return dst;
        }

        public static string ImageOCR(Bitmap image, bool debug = false)
        {
            using (TesseractEngine Engine = new TesseractEngine(OCRPath, OCRLang, OCRMode))
            {
                using (var t = new ResourcesTracker())
                {
                    Mat src = t.T(BitmapConverter.ToMat(image));
                    Mat enhanced = ImageEnhance(src);

                    if (debug) new Window("input image", enhanced);

                    using (Bitmap bmp = BitmapConverter.ToBitmap(enhanced))
                    {
                        using (Pix Pix = PixConverter.ToPix(bmp))
                        {
                            using (var page = Engine.Process(Pix))
                            {
                                string text = page.GetText();
                                return text;
                            }
                        }
                    }
                }
            }
        }

        public static string ImageOCR(string imagePath, bool debug = false)
        {
            try
            {
                using (Image Dummy = Image.FromFile(imagePath))
                {
                    return ImageOCR(new Bitmap(Dummy), debug);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(@"File not found.", e);
            }
        }
    }
}
