using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using Tesseract;

namespace Kikuzuki
{
    partial class TesseractOCR
    {
        private static readonly string OCRPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly EngineMode OCRMode = EngineMode.TesseractAndLstm;
        private static readonly PageIteratorLevel pageIteratorLevel = PageIteratorLevel.TextLine;

        public static string ImageOCR(Bitmap image, List<ImagePR> conf, string OCRLang = "eng",  bool debug = false)
        {
            using (TesseractEngine Engine = new TesseractEngine(OCRPath, OCRLang, OCRMode))
            {
                using (var t = new ResourcesTracker())
                {
                    Mat src = t.T(BitmapConverter.ToMat(image));
                    Mat enhanced = ImageEnhance(src, conf);

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

        public static OCRDetailed ImageOCRDetail(Bitmap image, List<ImagePR> conf, string OCRLang = "eng", bool debug = false)
        {
            using (TesseractEngine Engine = new TesseractEngine(OCRPath, OCRLang, OCRMode))
            {
                OCRDetailed output = new OCRDetailed();

                Mat src = BitmapConverter.ToMat(image);
                Mat enhanced = ImageEnhance(src, conf);

                using (Bitmap bmp = BitmapConverter.ToBitmap(enhanced))
                {
                    using (Pix Pix = PixConverter.ToPix(bmp))
                    {
                        using (var page = Engine.Process(Pix))
                        {
                            output.Text = page.GetText();
                            output.Boxes = page.GetSegmentedRegions(pageIteratorLevel);
                            
                            if (debug) output.BoxedSrc = DrawBoundingBox(bmp, output.Boxes);
                            else output.BoxedSrc = DrawBoundingBox(image, output.Boxes);
                            
                            return output;
                        }
                    }
                }
            }
        }
    }
}