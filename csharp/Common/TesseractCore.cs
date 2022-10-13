using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Tesseract;

namespace Kikuzuki
{
    partial class TesseractOCR
    {
        private static readonly string OCRPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly EngineMode OCRMode = EngineMode.TesseractAndLstm;
        private static readonly PageIteratorLevel pageIteratorLevel = PageIteratorLevel.TextLine;

        public static string ImageOCR(Bitmap image, List<ImagePR> conf, string from = "English")
        {
            string OCRLang = CheckLang(from);

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

        public static OCRDetailed ImageOCRDetail(Bitmap image, List<ImagePR> conf, Func<string, string, string, Task<string>> translator, string from = "English", string to = "Chinese Traditional", ProcessedType outConf = ProcessedType.IMAGE_BOXED)
        {
            string OCRLang = CheckLang(from);

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

                            string[] texts = output.Text.Replace("\n\n", "\n").Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                            switch (outConf)
                            {
                                case ProcessedType.IMAGE_BOXED:
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
                                case ProcessedType.IMAGE_PROCESSED:
                                    output.ProcessedSrc = DrawBoundingBox(bmp, output.Boxes);
                                    break;
                                case ProcessedType.IMAGE_REPLACED:
                                    if (conf.Contains(ImagePR.IMAGE_RESIZE))
                                    {
                                        if (image.Width <= 300 || image.Height <= 300)
                                        {
                                            float scale;

                                            if (image.Width > image.Height) scale = 300 / image.Height;
                                            else scale = 300 / image.Width;

                                            output.ProcessedSrc = ReplaceRectangle(ResizeImage(image, (int)(image.Width * scale), (int)(image.Height * scale)), output.Boxes, texts);
                                        }
                                        else output.ProcessedSrc = ReplaceRectangle(image, output.Boxes, texts);
                                    }
                                    else output.ProcessedSrc = ReplaceRectangle(image, output.Boxes, texts);
                                    break;
                                case ProcessedType.IMAGE_TRANSLATED:
                                    if (conf.Contains(ImagePR.IMAGE_RESIZE))
                                    {
                                        if (image.Width <= 300 || image.Height <= 300)
                                        {
                                            float scale;

                                            if (image.Width > image.Height) scale = 300 / image.Height;
                                            else scale = 300 / image.Width;

                                            output.ProcessedSrc = TranslateRectangle(ResizeImage(image, (int)(image.Width * scale), (int)(image.Height * scale)), output.Boxes, texts, from, to, translator);
                                        }
                                        else output.ProcessedSrc = TranslateRectangle(image, output.Boxes, texts, from, to, translator);
                                    }
                                    else output.ProcessedSrc = TranslateRectangle(image, output.Boxes, texts, from, to, translator);
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