using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kikuzuki
{
    internal class Program
    {
        static void Main()
        {
            TesseractOCR Ocr = new TesseractOCR(AppDomain.CurrentDomain.BaseDirectory, "eng");

            Console.WriteLine("OCR Test");
            Console.WriteLine(Ocr.ProcessScreen( new Point(0), new Point(0), new Size(300, 300 ), true ));
            
            Console.ReadKey();
        }
    }
}
