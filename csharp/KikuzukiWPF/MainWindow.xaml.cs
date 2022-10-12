using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using Kikuzuki;

namespace KikuzukiWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string OCRlang = "eng";
        private bool processed = false;
        private readonly List<TesseractOCR.ImagePR> conf = new List<TesseractOCR.ImagePR>();
        private TesseractOCR.OCROutput outConf = TesseractOCR.OCROutput.IMAGE_BOXED;

        public MainWindow()
        {
            InitializeComponent();

            foreach (TesseractOCR.PRListItem item in TesseractOCR.PRListItems)
            {
                PRListItems.Items.Add(item);

                if (item.IsSelected)
                {
                    conf.Add(item.Enum);
                }
            }
        }

        private void ToggleButtonPRItemClick(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)(sender as ToggleButton).IsChecked;
            TesseractOCR.PRListItem menuItem = (TesseractOCR.PRListItem)(sender as FrameworkElement).DataContext;

            foreach (TesseractOCR.PRListItem item in TesseractOCR.PRListItems)
            {
                if (menuItem.Name == item.Name)
                {
                    if (isChecked) conf.Add(item.Enum);
                    else conf.Remove(item.Enum);

                    break;
                }
            }

            if (processed)
            {
                Bitmap src = FormatHelper.ImageSource2Bitmap(imgSrc.Source);
                TesseractOCR.OCRDetailed det = TesseractOCR.ImageOCRDetail(src, conf, AzureTranslator.Translate, OCRlang, outConf);
                textDst.Text = det.Text;
                imgDst.Source = FormatHelper.Bitmap2ImageSource(det.ProcessedSrc);
            }
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
                Bitmap src = new Bitmap(dlg.FileName);
                imgSrc.Source = FormatHelper.Bitmap2ImageSource(src);

                TesseractOCR.OCRDetailed det = TesseractOCR.ImageOCRDetail(src, conf, AzureTranslator.Translate, OCRlang, outConf);
                textDst.Text = det.Text;
                imgDst.Source = FormatHelper.Bitmap2ImageSource(det.ProcessedSrc);
                processed = true;
            }
        }

        private void ButtonClipboardClick(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                BitmapSource clip = Clipboard.GetImage();
                Bitmap src = FormatHelper.BitmapSource2Bitmap(clip);

                imgSrc.Source = FormatHelper.Bitmap2ImageSource(src);

                TesseractOCR.OCRDetailed det = TesseractOCR.ImageOCRDetail(src, conf, AzureTranslator.Translate, OCRlang, outConf);
                textDst.Text = det.Text;
                imgDst.Source = FormatHelper.Bitmap2ImageSource(det.ProcessedSrc);
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
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imgDst.Source));
                    using (System.IO.FileStream stream = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                        encoder.Save(stream);
                }
            }
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)comboBoxLang.SelectedItem).Content is string content)
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
                    case "Korean":
                        OCRlang = "kor";
                        break;
                    default:
                        OCRlang = "eng";
                        break;
                }
            }
        }

        private void ComboBoxOutSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)comboBoxOutput.SelectedItem).Content is string content)
            {
                switch (content)
                {
                    case "Boxed":
                        outConf = TesseractOCR.OCROutput.IMAGE_BOXED;
                        break;
                    case "Processed":
                        outConf = TesseractOCR.OCROutput.IMAGE_PROCESSED;
                        break;
                    case "Replaced":
                        outConf = TesseractOCR.OCROutput.IMAGE_REPLACED;
                        break;
                    case "Translated":
                        outConf = TesseractOCR.OCROutput.IMAGE_TRANSLATED;
                        break;
                    default:
                        outConf = TesseractOCR.OCROutput.IMAGE_BOXED;
                        break;
                }
            }
        }
    }
}
