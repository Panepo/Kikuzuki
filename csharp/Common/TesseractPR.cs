using OpenCvSharp;
using OpenCvSharp.XImgProc;
using OpenCvSharp.XPhoto;
using System.Collections.Generic;

namespace Kikuzuki
{
    partial class TesseractOCR
    {
        private static Mat ImageEnhance(Mat src, List<ImagePR> conf)
        {
            Mat dst = src.Clone();

            foreach (var pr in conf)
            {
                foreach (PRListItem item in PRListItems)
                {
                    if (pr == item.Enum)
                    {
                        Mat temp = new Mat();
                        temp = item.Func(dst);

                        dst = new Mat();
                        dst = temp.Clone();

                        break;
                    }
                }
            }

            return dst;
        }

        private static Mat ImageResize(Mat src)
        {
            if (src.Width <= 300 || src.Height <= 300)
            {
                Mat dst = new Mat();
                OpenCvSharp.Size size = new OpenCvSharp.Size(src.Width * 2, src.Height * 2);
                Cv2.Resize(src, dst, size, 0, 0, InterpolationFlags.Cubic);
                return dst;
            }
            else return src;
        }

        private static Mat ImageBinarization(Mat src)
        {
            Mat gray = new Mat();
            Mat dst = new Mat();

            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(gray, dst, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            return dst;
        }

        private static Mat ImageDenoise(Mat src)
        {
            Mat dst = new Mat();
            Mat temp = new Mat();

            Cv2.CvtColor(src, temp, ColorConversionCodes.RGB2GRAY);
            Cv2.CvtColor(temp, temp, ColorConversionCodes.GRAY2RGB);
            
            Cv2.FastNlMeansDenoisingColored(temp, dst, (float) 5.5);
            return dst;
        }

        private static Mat ImageDeblur(Mat src)
        {
            Mat dst = new Mat();

            Cv2.Split(src, out Mat[] channels);

            foreach (Mat channel in channels)
            {
                Mat channelFloat = new Mat();
                channel.ConvertTo(channelFloat, MatType.CV_32F);

                Cv2.GaussianBlur(channelFloat, channel, new OpenCvSharp.Size(11, 11), 10, 30);
                Cv2.AddWeighted(channelFloat, 2.0 * 0.75, channel, 2.0 * 0.75 - 2.0, 0.0, channel);

                channel.ConvertTo(channel, MatType.CV_8U);
            }

            Cv2.Merge(channels, dst);
            
            return dst;
        }

        private static Mat ImageSmooth(Mat src)
        {
            Mat dst = src.Clone();
            Cv2.MedianBlur(src, dst, 5);
            return dst;
        }

        private static Mat ImageWhiteBalance(Mat src)
        {
            Mat dst = new Mat();
            SimpleWB wb = CvXPhoto.CreateSimpleWB();
            wb.BalanceWhite(src, dst);
            return dst;
        }

        private static Mat ImageThinning(Mat src)
        {
            Mat dst = new Mat();
            CvXImgProc.Thinning(src, dst, ThinningTypes.ZHANGSUEN);
            return dst;
        }
    }
}