using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Kikuzuki
{
    partial class TesseractOCR
    {
        public struct OCRDetailed
        {
            public string Text;
            public List<Rectangle> Boxes;
            public Bitmap BoxedSrc;
        }

        public static string ImageOCR(string imagePath, List<ImagePR> conf, string OCRLang = "eng", bool debug = false)
        {
            try
            {
                using (Image Dummy = Image.FromFile(imagePath))
                {
                    return ImageOCR(new Bitmap(Dummy), conf, OCRLang, debug);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(@"File not found.", e);
            }
        }

        public static OCRDetailed ImageOCRDetail(string imagePath, List<ImagePR> conf, string OCRLang = "eng", bool debug = false)
        {
            try
            {
                using (Image Dummy = Image.FromFile(imagePath))
                {
                    return ImageOCRDetail(new Bitmap(Dummy), conf, OCRLang, debug);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(@"File not found.", e);
            }
        }
    }
}