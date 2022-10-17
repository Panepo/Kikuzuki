using Kikuzuki;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

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
        private string OCRlang = "English";
        private string TransLang = "Chinese Traditional";
        private bool processed = false;
        private readonly List<TesseractOCR.ImagePR> conf = new List<TesseractOCR.ImagePR>();
        private TesseractOCR.ProcessedType outConf = TesseractOCR.ProcessedType.IMAGE_BOXED;

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

            foreach(TesseractOCR.LangData lang in TesseractOCR.LangDatas)
            {
                comboBoxLang.Items.Add(lang.Name);
            }
            foreach (string item in comboBoxLang.Items)
            {
                if (item == "English")
                {
                    comboBoxLang.SelectedValue = item;
                    break;
                }
            }

            foreach (TesseractOCR.ProcessedList process in TesseractOCR.ProcessedLists)
            {
                comboBoxOutput.Items.Add(process.Name);
            }
            foreach (string item in comboBoxOutput.Items)
            {
                if (item == "Boxed")
                {
                    comboBoxOutput.SelectedValue = item;
                    break;
                }
            }

            foreach (AzureTranslator.LangData lang in AzureTranslator.LangDatas)
            {
                comboBoxTrans.Items.Add(lang.Name);
            }
            foreach (string item in comboBoxTrans.Items)
            {
                if (item == "Chinese Traditional")
                {
                    comboBoxTrans.SelectedValue = item;
                    break;
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
                TesseractOCR.OCRDetailed det = TesseractOCR.ImageOCRDetail(src, conf, AzureTranslator.Translate, OCRlang, TransLang, outConf);
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

                TesseractOCR.OCRDetailed det = TesseractOCR.ImageOCRDetail(src, conf, AzureTranslator.Translate, OCRlang, TransLang, outConf);
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

                TesseractOCR.OCRDetailed det = TesseractOCR.ImageOCRDetail(src, conf, AzureTranslator.Translate, OCRlang, TransLang, outConf);
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
            if (comboBoxLang.SelectedItem is string content)
            {
                OCRlang = content;
            }
        }

        private void ComboBoxOutSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxOutput.SelectedItem is string content)
            {
                foreach (TesseractOCR.ProcessedList process in TesseractOCR.ProcessedLists)
                {
                    if (content == process.Name)
                    {
                        outConf = process.Code;

                        if (content == "Text Translated") comboBoxTrans.IsEnabled = true;
                        else comboBoxTrans.IsEnabled = false;
                        break;
                    }
                }
            }
        }

        private void ComboBoxTransSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxTrans.SelectedItem is string content)
            {
                TransLang = content;
            }
        }
    }
}
