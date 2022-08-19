using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kikuzuki;

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

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            OpenFileDialog dlg = new OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.Filter = "Image Files(*.png; *.jpg; *.jpeg; *.gif; *.bmp)|*.png; *.jpg; *.jpeg; *.gif; *.bmp";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                Bitmap src = new Bitmap(dlg.FileName);
                imgSrc.Source = FormatHelper.Bitmap2ImageSource(src);

                TesseractOCR.OCRDetailed det = TesseractOCR.ImageOCRDetail(src);
                textDst.Text = det.Text.Replace("\n", " | ").Replace("\r", " | "); ;
                imgDst.Source = FormatHelper.Bitmap2ImageSource(det.BoxedSrc);
            }
        }
    }
}
