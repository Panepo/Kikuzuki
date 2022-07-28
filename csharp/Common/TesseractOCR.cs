using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public string ProcessBMP(string imagePath)
        {
            Pix Pix = PixConverter.ToPix(new Bitmap(imagePath));
            using (var page = Engine.Process(Pix))
            {
                string text = page.GetText();
                return text;
            }
        }
    }
}