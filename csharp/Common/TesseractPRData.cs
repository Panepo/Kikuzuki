using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace Kikuzuki
{
    partial class TesseractOCR
    {
        public enum ImagePR
        {
            IMAGE_RESIZE,
            IMAGE_ROTATE,
            IMAGE_BINARIZATION,
            IMAGE_BINARIZATION_INVERSE,
            IMAGE_DENOISE,
            IMAGE_DEBLUR,
            IMAGE_SMOOTH,
            IMAGE_WHITEBALANCE,
            IMAGE_THINNING
        }

        public class PRListItem
        {
            public bool IsSelected { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public char Code { get; set; }
            public ImagePR Enum { get; set; }
            public Func<Mat, Mat> Func { get; set; }
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
                Code = 'I',
                Name = "Inverse",
                Description = "Turn the image to black and white and inverse it",
                Enum = ImagePR.IMAGE_BINARIZATION_INVERSE,
                Func = ImageBinarizationInverse
            },
            /* new PRListItem
            {
                Code = 'N',
                Name = "Denoise",
                Description = "Remove noise from image",
                Enum = ImagePR.IMAGE_DENOISE,
                Func = ImageDenoise
            }, */
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
                IsSelected = true,
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
            /*new PRListItem
            {
                Code = 'T',
                Name = "Thinnging",
                Description = "Thinnging the image",
                Enum = ImagePR.IMAGE_THINNING,
                Func = ImageThinning
            }*/
        };
    }
}