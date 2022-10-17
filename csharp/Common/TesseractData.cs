using System.Collections.Generic;
using System.Drawing;

namespace Kikuzuki
{
    partial class TesseractOCR
    {
        #region Language
        public class LangData
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }

        public static List<LangData> LangDatas = new List<LangData>()
        {
            new LangData
            {
                Name = "English",
                Code = "eng"
            },
            new LangData
            {
                Name = "Chinese Traditional",
                Code = "chi_tra"
            },
            new LangData
            {
                Name = "Japanese",
                Code = "jpn"
            },
            new LangData
            {
                Name = "Korean",
                Code = "kor"
            },
        };

        private static string CheckLang(string name)
        {
            foreach (LangData lang in LangDatas)
            {
                if (lang.Name == name)
                {
                    return lang.Code;
                }
            }

            return LangDatas[0].Code;
        }
        #endregion

        #region Detailed Output
        public struct OCRDetailed
        {
            public string Text;
            public List<Rectangle> Boxes;
            public Bitmap ProcessedSrc;
        }

        public enum ProcessedType
        {
            IMAGE_BOXED,
            IMAGE_PROCESSED,
            IMAGE_REPLACED,
            IMAGE_TRANSLATED
        }

        public class ProcessedList
        {
            public string Name { get; set; }
            public ProcessedType Code { get; set; }
        }

        public static List<ProcessedList> ProcessedLists = new List<ProcessedList>()
        {
            new ProcessedList
            {
                Name = "Boxed",
                Code = ProcessedType.IMAGE_BOXED
            },
            new ProcessedList
            {
                Name = "Image Processed",
                Code = ProcessedType.IMAGE_PROCESSED
            },
            new ProcessedList
            {
                Name = "Text Replaced",
                Code = ProcessedType.IMAGE_REPLACED
            },
            new ProcessedList
            {
                Name = "Text Translated",
                Code = ProcessedType.IMAGE_TRANSLATED
            },
        };
        #endregion
    }
}