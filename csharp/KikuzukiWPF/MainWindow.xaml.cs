using Kikuzuki;
using System;
using System.Windows;

namespace KikuzukiWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonFileClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image Files(*.png; *.jpg; *.jpeg; *.gif; *.bmp)|*.png; *.jpg; *.jpeg; *.gif; *.bmp";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                System.Drawing.Bitmap src = new System.Drawing.Bitmap(dlg.FileName);
                imgSrc.Source = FormatHelper.Bitmap2ImageSource(src);

                TesseractOCR.OCRDetailed det = TesseractOCR.ImageOCRDetail(src);
                textDst.Text = det.Text.Replace("\n", " | ").Replace("\r", " | "); ;
                imgDst.Source = FormatHelper.Bitmap2ImageSource(det.BoxedSrc);
            }
        }

        private void ButtonClipboardClick(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                System.Windows.Media.Imaging.BitmapSource clip = Clipboard.GetImage();
                System.Drawing.Bitmap src = FormatHelper.BitmapSource2Bitmap(clip);

                imgSrc.Source = FormatHelper.Bitmap2ImageSource(src);

                TesseractOCR.OCRDetailed det = TesseractOCR.ImageOCRDetail(src);
                textDst.Text = det.Text.Replace("\n", " | ").Replace("\r", " | "); ;
                imgDst.Source = FormatHelper.Bitmap2ImageSource(det.BoxedSrc);
            }
        }
    }
}
