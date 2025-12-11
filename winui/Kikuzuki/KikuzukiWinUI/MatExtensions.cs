using Microsoft.UI.Xaml.Media.Imaging;
using OpenCvSharp;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

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
}