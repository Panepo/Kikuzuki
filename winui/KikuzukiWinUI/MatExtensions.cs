using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.AI.Imaging;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace Kikuzuki;
public static class MatExtensions
{
    public static WriteableBitmap ToWriteableBitmap(this Mat mat)
    {
        Mat bgraMat = mat.Type() == MatType.CV_8UC4 ? mat : mat.CvtColor(ColorConversionCodes.BGR2BGRA);

        var wbmp = new WriteableBitmap(bgraMat.Width, bgraMat.Height);

        byte[] buffer = bgraMat.GetBuffer();
        wbmp.PixelBuffer.AsStream().Write(buffer, 0, buffer.Length);

        return wbmp;
    }

    private static byte[] GetBuffer(this Mat mat)
    {
        var bytes = new byte[mat.Rows * mat.Cols * mat.ElemSize()];
        Marshal.Copy(mat.Data, bytes, 0, bytes.Length);
        return bytes;
    }

    public static SoftwareBitmap ToSoftwareBitmap(this Mat mat)
    {
        if (mat == null || mat.Empty())
            throw new ArgumentNullException(nameof(mat));

        // Convert Mat to BGRA if not already
        Mat bgraMat = mat.Type().Channels == 4
            ? mat
            : mat.CvtColor(OpenCvSharp.ColorConversionCodes.BGR2BGRA);

        // Get pixel data
        int width = bgraMat.Width;
        int height = bgraMat.Height;
        int stride = width * 4;
        byte[] pixelData = new byte[height * stride];
        System.Runtime.InteropServices.Marshal.Copy(bgraMat.Data, pixelData, 0, pixelData.Length);

        // Create SoftwareBitmap
        var bitmap = SoftwareBitmap.CreateCopyFromBuffer(
            WindowsRuntimeBufferExtensions.AsBuffer(pixelData, 0, pixelData.Length),
            BitmapPixelFormat.Bgra8,
            width,
            height,
            BitmapAlphaMode.Premultiplied);

        // If a conversion was made, dispose the temporary Mat
        if (!ReferenceEquals(bgraMat, mat))
            bgraMat.Dispose();

        return bitmap;
    }

    public static Mat DrawRecognizedText(this Mat mat, RecognizedText recognizedText)
    {
        Mat dst = mat.Clone();
        foreach (RecognizedLine line in recognizedText.Lines)
        {
            // Extract the four points from the bounding box
            var bbox = line.BoundingBox;
            var points = new[]
            {
                    new OpenCvSharp.Point(bbox.TopLeft.X, bbox.TopLeft.Y),
                    new OpenCvSharp.Point(bbox.TopRight.X, bbox.TopRight.Y),
                    new OpenCvSharp.Point(bbox.BottomRight.X, bbox.BottomRight.Y),
                    new OpenCvSharp.Point(bbox.BottomLeft.X, bbox.BottomLeft.Y)
                };
            Cv2.Polylines(dst, [points], isClosed: true, color: Scalar.Red, thickness: 2);
            // Draw the recognized text above the bounding box
            var textPosition = new OpenCvSharp.Point(points[0].X, points[0].Y - 10);
            Cv2.PutText(dst, line.Text, textPosition, HersheyFonts.HersheySimplex, 0.7, Scalar.Blue, 2);
        }
        return dst;
    }

    public static Bitmap ToBitmap(this Mat mat)
    {
        if (mat == null || mat.Empty())
            throw new ArgumentNullException(nameof(mat));

        // Convert to BGRA if not already 4 channels
        Mat bgraMat = mat.Type().Channels == 4
            ? mat
            : mat.CvtColor(OpenCvSharp.ColorConversionCodes.BGR2BGRA);

        Bitmap bitmap = new(bgraMat.Width, bgraMat.Height, PixelFormat.Format32bppArgb);

        BitmapData data = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);

        // Copy data from Mat to Bitmap
        // Marshal.Copy(byte[] source, int startIndex, IntPtr destination, int length)
        byte[] buffer = new byte[bgraMat.Width * bgraMat.Height * 4];
        Marshal.Copy(bgraMat.Data, buffer, 0, buffer.Length);
        Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);

        bitmap.UnlockBits(data);

        // Dispose temporary Mat if conversion was made
        if (!ReferenceEquals(bgraMat, mat))
            bgraMat.Dispose();

        return bitmap;
    }
}