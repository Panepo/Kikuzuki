using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kikuzuki
{
    internal class Program
    {
        static void Main()
        {
            TesseractOCR Ocr = new TesseractOCR(AppDomain.CurrentDomain.BaseDirectory, "eng");
            Console.WriteLine(Ocr.ProcessBMP("1.bmp"));
            Console.ReadKey();
        }
    }
}
