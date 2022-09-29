using System;
using System.Collections.Generic;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.XPhoto;

namespace Kikuzuki
{
    partial class TesseractOCR
    {
        public enum ImagePR
        {
            IMAGE_RESIZE,
            IMAGE_ROTATE,
            IMAGE_BINARIZATION,
            IMAGE_DENOISE,
            IMAGE_DEBLUR,
            IMAGE_SMOOTH,
            IMAGE_WHITEBALANCE,
            IMAGE_THINNING
        }

        public class PRListItem
        {
            private bool _isSelected;
            private string _name;
            private string _description;
            private char _code;
            private ImagePR _enum;
            private Func<Mat, Mat> _func;

            public bool IsSelected
            {
                get => _isSelected;
                set => _isSelected = value;
            }

            public char Code
            {
                get => _code;
                set => _code = value;
            }

            public string Name
            {
                get => _name;
                set => _name = value;
            }

            public string Description
            {
                get => _description;
                set => _description = value;
            }

            public ImagePR Enum
            {
                get => _enum;
                set => _enum = value;
            }

            public Func<Mat, Mat> Func
            {
                get => _func;
                set => _func = value;
            }
        }

        public static List<PRListItem> PRListItems = new List<PRListItem>()
        {
            new PRListItem
            {
                Code = 'R',
                Name = "Resize",
                Description = "Resize the image to approriate size",
                IsSelected = true,
                Enum = ImagePR.IMAGE_RESIZE,
                Func = ImageResize
            },
            new PRListItem
            {
                Code = 'B',
                Name = "Binarization",
                Description = "Turn the image to black and white",
                IsSelected = true,
                Enum = ImagePR.IMAGE_BINARIZATION,
                Func = ImageBinarization
            },
            new PRListItem
            {
                Code = 'N',
                Name = "Denoise",
                Description = "Remove noise from image",
                Enum = ImagePR.IMAGE_DENOISE,
                Func = ImageDenoise
            },
            new PRListItem
            {
                Code = 'D',
                Name = "Deblur",
                Description = "Remove blur from image",
                Enum = ImagePR.IMAGE_DEBLUR,
                Func = ImageDeblur
            },
            new PRListItem
            {
                Code = 'S',
                Name = "Smooth",
                Description = "Smooth the image",
                Enum = ImagePR.IMAGE_SMOOTH,
                Func = ImageSmooth
            },
            new PRListItem
            {
                Code = 'W',
                Name = "White Balance",
                Description = "Run white balance algorithm",
                Enum = ImagePR.IMAGE_WHITEBALANCE,
                Func = ImageWhiteBalance
            },
            new PRListItem
            {
                Code = 'T',
                Name = "Thinnging",
                Description = "Thinnging the image",
                Enum = ImagePR.IMAGE_THINNING,
                Func = ImageThinning
            }
        };
    }
}