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
        private string OCRlang = "eng";
        private bool processed = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonFileClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files(*.png; *.jpg; *.jpeg; *.gif; *.bmp)|*.png; *.jpg; *.jpeg; *.gif; *.bmp"
            };
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                System.Drawing.Bitmap src = new System.Drawing.Bitmap(dlg.FileName);
                imgSrc.Source = FormatHelper.Bitmap2ImageSource(src);

                TesseractOCR.OCRDetailed det = TesseractOCR.ImageOCRDetail(src);
                textDst.Text = det.Text;
                imgDst.Source = FormatHelper.Bitmap2ImageSource(det.BoxedSrc);
                processed = true;
            }
        }

        private void ButtonClipboardClick(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                System.Windows.Media.Imaging.BitmapSource clip = Clipboard.GetImage();
                System.Drawing.Bitmap src = FormatHelper.BitmapSource2Bitmap(clip);

                imgSrc.Source = FormatHelper.Bitmap2ImageSource(src);

                TesseractOCR.OCRDetailed det = TesseractOCR.ImageOCRDetail(src, OCRlang);
                textDst.Text = det.Text;
                imgDst.Source = FormatHelper.Bitmap2ImageSource(det.BoxedSrc);
                processed = true;
            }
        }

        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            if (processed)
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Images|*.png;*.bmp;*.jpg"
                };
                Nullable<bool> result = sfd.ShowDialog();

                if (result == true)
                {
                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create((System.Windows.Media.Imaging.BitmapSource)imgDst.Source));
                    using (System.IO.FileStream stream = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                    encoder.Save(stream);
                }
            }
        }

        private void ComboBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (((System.Windows.Controls.ComboBoxItem)comboBoxLang.SelectedItem).Content is string content)
            {
                switch (content)
                {
                    case "English":
                        OCRlang = "eng";
                        break;
                    case "Chinese":
                        OCRlang = "chi_tra";
                        break;
                    case "Japanese":
                        OCRlang = "jpn";
                        break;
                    default:
                        OCRlang = "eng";
                        break;
                }
            }
        }
    }
}
