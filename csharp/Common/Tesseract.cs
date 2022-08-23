using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using Tesseract;

namespace Kikuzuki
{
    public class TesseractOCR
    {
        private static readonly string OCRPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly EngineMode OCRMode = EngineMode.TesseractAndLstm;
        private static readonly PageIteratorLevel pageIteratorLevel = PageIteratorLevel.TextLine;

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

        public static string ImageOCR(System.Drawing.Bitmap image, string OCRLang = "eng", bool debug = false)
        {
            using (TesseractEngine Engine = new TesseractEngine(OCRPath, OCRLang, OCRMode))
            {
                using (var t = new ResourcesTracker())
                {
                    Mat src = t.T(BitmapConverter.ToMat(image));
                    Mat enhanced = ImageEnhance(src);

                    if (debug) new Window("input image", enhanced);

                    using (System.Drawing.Bitmap bmp = BitmapConverter.ToBitmap(enhanced))
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

        public static string ImageOCR(string imagePath, string OCRLang = "eng", bool debug = false)
        {
            try
            {
                using (System.Drawing.Image Dummy = System.Drawing.Image.FromFile(imagePath))
                {
                    return ImageOCR(new System.Drawing.Bitmap(Dummy), OCRLang, debug);
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                throw new System.IO.FileNotFoundException(@"File not found.", e);
            }
        }

        public struct OCRDetailed
        {
            public string Text;
            public System.Collections.Generic.List<System.Drawing.Rectangle> Boxes;
            public System.Drawing.Bitmap BoxedSrc;
        }

        public static OCRDetailed ImageOCRDetail(System.Drawing.Bitmap image, string OCRLang = "eng", bool debug = false)
        {
            using (TesseractEngine Engine = new TesseractEngine(OCRPath, OCRLang, OCRMode))
            {
                OCRDetailed output = new OCRDetailed();

                using (var t = new ResourcesTracker())
                {
                    Mat src = t.T(BitmapConverter.ToMat(image));
                    Mat enhanced = ImageEnhance(src);

                    if (debug) new Window("input image", enhanced);

                    using (System.Drawing.Bitmap bmp = BitmapConverter.ToBitmap(enhanced))
                    {
                        using (Pix Pix = PixConverter.ToPix(bmp))
                        {
                            using (var page = Engine.Process(Pix))
                            {
                                output.Text = page.GetText();
                                output.Boxes = page.GetSegmentedRegions(pageIteratorLevel);
                                output.BoxedSrc = DrawBoundingBox(image, output.Boxes);
                                return output;
                            }
                        }
                    }
                }
            }
        }

        private static System.Drawing.Bitmap DrawBoundingBox(System.Drawing.Bitmap src, System.Collections.Generic.List<System.Drawing.Rectangle> boxes)
        {
            System.Drawing.Bitmap dst = new System.Drawing.Bitmap(src);
            using (System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(dst))
            {
                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 2);
                boxes.ForEach(box =>
                {
                    graphic.DrawRectangle(pen, box);
                });

                return dst;
            }
        }
    }
}
