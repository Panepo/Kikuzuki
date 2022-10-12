using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Kikuzuki
{
    partial class TesseractOCR
    {
        public struct OCRDetailed
        {
            public string Text;
            public List<Rectangle> Boxes;
            public Bitmap ProcessedSrc;
        }

        public enum OCROutput
        {
            IMAGE_BOXED,
            IMAGE_PROCESSED,
            IMAGE_REPLACED,
            IMAGE_TRANSLATED
        }

        public static string ImageOCR(string imagePath, List<ImagePR> conf, string OCRLang = "eng")
        {
            try
            {
                using (Image Dummy = Image.FromFile(imagePath))
                {
                    return ImageOCR(new Bitmap(Dummy), conf, OCRLang);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(@"File not found.", e);
            }
        }

        public static OCRDetailed ImageOCRDetail(string imagePath, List<ImagePR> conf, Func<string, Task<string>> translator, string OCRLang = "eng", OCROutput outConf = OCROutput.IMAGE_BOXED)
        {
            try
            {
                using (Image Dummy = Image.FromFile(imagePath))
                {
                    return ImageOCRDetail(new Bitmap(Dummy), conf, translator, OCRLang, outConf);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(@"File not found.", e);
            }
        }
    }
}