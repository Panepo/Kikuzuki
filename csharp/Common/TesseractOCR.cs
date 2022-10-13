using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Kikuzuki
{
    partial class TesseractOCR
    {
        public static string ImageOCR(string imagePath, List<ImagePR> conf, string from = "English")
        {
            try
            {
                using (Image Dummy = Image.FromFile(imagePath))
                {
                    return ImageOCR(new Bitmap(Dummy), conf, from);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(@"File not found.", e);
            }
        }

        public static OCRDetailed ImageOCRDetail(string imagePath, List<ImagePR> conf, Func<string, string, string, Task<string>> translator, string from = "English", string to = "Chinese Traditional", ProcessedType outConf = ProcessedType.IMAGE_BOXED)
        {
            try
            {
                using (Image Dummy = Image.FromFile(imagePath))
                {
                    return ImageOCRDetail(new Bitmap(Dummy), conf, translator, from, to, outConf);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(@"File not found.", e);
            }
        }
    }
}