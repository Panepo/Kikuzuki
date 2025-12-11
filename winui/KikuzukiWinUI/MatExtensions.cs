using Microsoft.UI.Xaml.Media.Imaging;
using OpenCvSharp;
using System;
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
}