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

        public static string ImageOCR(Bitmap image, List<ImagePR> conf, string OCRLang = "eng")
        {
            using (TesseractEngine Engine = new TesseractEngine(OCRPath, OCRLang, OCRMode))
            {
                using (var t = new ResourcesTracker())
                {
                    Mat src = t.T(BitmapConverter.ToMat(image));
                    Mat enhanced = ImageEnhance(src, conf);

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

        public static OCRDetailed ImageOCRDetail(Bitmap image, List<ImagePR> conf, string OCRLang = "eng", OCROutput outConf = OCROutput.IMAGE_BOXED)
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

                            switch (outConf)
                            {
                                case OCROutput.IMAGE_BOXED:
                                    if (conf.Contains(ImagePR.IMAGE_RESIZE))
                                    {
                                        if (image.Width <= 300 || image.Height <= 300)
                                        {
                                            float scale;

                                            if (image.Width > image.Height) scale = 300 / image.Height;
                                            else scale = 300 / image.Width;

                                            output.ProcessedSrc = DrawBoundingBox(ResizeImage(image, (int)(image.Width * scale), (int)(image.Height * scale)), output.Boxes);
                                        }
                                        else output.ProcessedSrc = DrawBoundingBox(image, output.Boxes);
                                    }
                                    else output.ProcessedSrc = DrawBoundingBox(image, output.Boxes);
                                    break;
                                case OCROutput.IMAGE_PROCESSED:
                                    output.ProcessedSrc = DrawBoundingBox(bmp, output.Boxes);
                                    break;
                                case OCROutput.IMAGE_REPLACED:
                                    string[] texts = output.Text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                                    if (conf.Contains(ImagePR.IMAGE_RESIZE))
                                    {
                                        if (image.Width <= 300 || image.Height <= 300)
                                        {
                                            float scale;

                                            if (image.Width > image.Height) scale = 300 / image.Height;
                                            else scale = 300 / image.Width;

                                            output.ProcessedSrc = ReplaceBoundingBox(ResizeImage(image, (int)(image.Width * scale), (int)(image.Height * scale)), output.Boxes, texts);
                                        }
                                        else output.ProcessedSrc = ReplaceBoundingBox(image, output.Boxes, texts);
                                    }
                                    else output.ProcessedSrc = ReplaceBoundingBox(image, output.Boxes, texts);
                                    break;
                            }
                            
                            return output;
                        }
                    }
                }
            }
        }
    }
}