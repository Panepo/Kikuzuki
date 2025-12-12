using Kikuzuki;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.AI.Imaging;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using static Kikuzuki.CameraEnumerator;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace KikuzukiWinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Microsoft.UI.Xaml.Window
    {
        private static readonly Mat _blackMat = new(480, 600, MatType.CV_8UC3, Scalar.Black);

        private CameraDevice _selCamera;
        private Camera? _cam;
        private Mat _frame = _blackMat;
        private bool _isStreaming = false;
        
        private Mat _capturedFrame = _blackMat;

        private ImageDescClient? _imgDescClient;
        private bool _isRecognizing = false;

        private TextRecoClient? _textRecoClient;
        private bool _isRecognizingText = false;

        private TextTransClient? _textTransClient;
        private bool _isTranslatingText = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDevice();
            InitializeFunction();

            ImgCamera.Source = _frame.ToWriteableBitmap();

            this.AppWindow.Resize(new SizeInt32(755, 745));
        }

        private void InitializeDevice()
        {
            foreach (CameraDevice camera in CameraEnumerator.Cameras)
            {
                ComboBoxCamera.Items.Add(camera.deviceName);
            }
            if (CameraEnumerator.Cameras.Length > 0)
            {
                ComboBoxCamera.SelectedIndex = 0;
                _selCamera = Cameras[0];
            }
        }

        private void InitializeFunction()
        {
            _imgDescClient = new ImageDescClient();
            _imgDescClient.OnProcessed += (mObjct, mArgs) =>
            {
                if (mArgs.Status == ImageDescClient.Status.End)
                {
                    _isRecognizing = false;
                    _ = DispatcherQueue.TryEnqueue(() =>
                    {
                        ButtonRecognizeText.Text = "Recognize";
                        TextRecognized.Text = mArgs.Output;
                    });
                }
            };

            _textRecoClient = new TextRecoClient();

            _textTransClient = new TextTransClient();
            foreach (string lang in _textTransClient.languages)
            {
                ComboBoxLang.Items.Add(lang);
            }
        }

        private void ComboBoxCameraChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedCamera = (string)ComboBoxCamera.SelectedValue;

            for (var i = 0; i < CameraEnumerator.Cameras.Length; i++)
            {
                if (selectedCamera == CameraEnumerator.Cameras[i].deviceName)
                {
                    this._selCamera = CameraEnumerator.Cameras[i];
                    break;
                }
            }
        }

        private async void ButtonOpenClick(object sender, RoutedEventArgs e)
        {
            var window = new Microsoft.UI.Xaml.Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            var picker = new FileOpenPicker();

            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".jpg");

            picker.ViewMode = PickerViewMode.Thumbnail;

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using var stream = await file.OpenReadAsync();
                await SetImage(stream);
            }
        }

        private async Task SetImage(string filePath)
        {
            if (File.Exists(filePath))
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
                using IRandomAccessStream stream = await file.OpenReadAsync();
                await SetImage(stream);
            }
        }

        private async Task SetImage(IRandomAccessStream stream)
        {
            var decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap inputBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            if (inputBitmap == null)
            {
                return;
            }

            var bitmapSource = new SoftwareBitmapSource();

            // This conversion ensures that the image is Bgra8 and Premultiplied
            SoftwareBitmap convertedImage = SoftwareBitmap.Convert(inputBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            await bitmapSource.SetBitmapAsync(convertedImage);
            ImgCamera.Source = bitmapSource;
            _capturedFrame = ImageFormatExtensions.SoftwareBitmapToMatAsync(convertedImage);

            ButtonRecognize.IsEnabled = true;
            ButtonOCR.IsEnabled = true;
            ButtonTrans.IsEnabled = true;
            TextRecognized.Text = string.Empty;
        }

        private async void ButtonCopyClick(object sender, RoutedEventArgs e)
        {
            var package = Clipboard.GetContent();
            if (package.Contains(StandardDataFormats.Bitmap))
            {
                RectCanvas.Visibility = Visibility.Collapsed;
                var streamRef = await package.GetBitmapAsync();

                IRandomAccessStream stream = await streamRef.OpenReadAsync();
                await SetImage(stream);
            }
            else if (package.Contains(StandardDataFormats.StorageItems))
            {
                var storageItems = await package.GetStorageItemsAsync();
                if (IsImageFile(storageItems[0].Path))
                {
                    try
                    {
                        var storageFile = await StorageFile.GetFileFromPathAsync(storageItems[0].Path);
                        using var stream = await storageFile.OpenReadAsync();
                        await SetImage(stream);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Invalid image file");
                    }
                }
            }
        }

        private static bool IsImageFile(string fileName)
        {
            string[] imageExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif"];
            return imageExtensions.Contains(System.IO.Path.GetExtension(fileName)?.ToLowerInvariant());
        }

        private void ProcessFrame(object? sender, object? args)
        {
            if (_cam == null) throw new Exception("Camera not initialized.");

            _frame = _cam.FrameCapture();

            if (_frame != null)
            {
                ImageSource bmp = _frame.ToWriteableBitmap();
                ImgCamera.Source = bmp;
            }
        }

        private void ButtonCameraClick(object sender, RoutedEventArgs e)
        {
            if (_isStreaming)
            {
                if (_frame != null)
                {
                    _isStreaming = false;
                    ButtonCameraText.Text = "Camera Restart";

                    ButtonRecognize.IsEnabled = true;
                    ButtonOCR.IsEnabled = true;
                    ButtonTrans.IsEnabled = true;
                    TextRecognized.Text = string.Empty;

                    _capturedFrame = _frame.Clone();
                    ImageSource bmp = _frame.ToWriteableBitmap();
                    ImgCamera.Source = bmp;

                    _cam?.StopTimer();
                    _cam?.ReleaseCamera();
                }
            }
            else
            {
                _isStreaming = true;
                ButtonCameraText.Text = "Capture";

                _capturedFrame = _blackMat;

                _cam = new Camera(_selCamera.deviceID, new EventHandler<object>(ProcessFrame));
                _cam.StartTimer();
            }
        }

        private async void ButtonRecognizeClick(object sender, RoutedEventArgs e)
        {
            if (_imgDescClient == null) throw new Exception("Image Description Client not initialized.");
            if (_capturedFrame == null) throw new Exception("No captured frame available.");

            if (_isRecognizing)
            {
                _isRecognizing = false;
                ButtonRecognizeText.Text = "Describe";
                _imgDescClient.StopDescribing();
            }
            else
            {
                _isRecognizing = true;
                ButtonRecognizeText.Text = "Stop";
                await _imgDescClient.DescribeImage(_capturedFrame.ToSoftwareBitmap());
            }
        }

        private async void ButtonOCRClick(object sender, RoutedEventArgs e)
        {
            if (_textRecoClient == null) throw new Exception("Text Recognition Client not initialized.");
            if (_capturedFrame == null) throw new Exception("No captured frame available.");

            if (_isRecognizingText)
            {
                _isRecognizingText = false;
                ButtonOCRText.Text = "Recognize";
            }
            else
            {
                _isRecognizingText = true;
                ButtonOCRText.Text = "Stop";
                
                RecognizedText recognizedText = await _textRecoClient.RecognizeTextAsync(_capturedFrame.ToSoftwareBitmap());
                TextRecoClient.RecognizedTextToBoxesAndTexts(recognizedText, out List<System.Drawing.Rectangle> boxes, out string[] texts);

                Bitmap drawnBitmap = ImageUtils.DrawRectangleAndText(
                    _capturedFrame.ToBitmap(),
                    boxes,
                    texts);
                ImgCamera.Source = ImageFormatExtensions.Bitmap2ImageSource(drawnBitmap);

                _isRecognizingText = false;
                ButtonOCRText.Text = "Recognize";
                TextRecognized.Text = string.Join('\n', texts);
            }
        }

        private async void ButtonTransClick(object sender, RoutedEventArgs e)
        {
            if (_textRecoClient == null) throw new Exception("Text Recognition Client not initialized.");
            if (_textTransClient == null) throw new Exception("Text Translation Client not initialized.");
            if (_isTranslatingText)
            {
                _isTranslatingText = false;
                ButtonTransText.Text = "Translate";
            }
            else
            {
                _isTranslatingText = true;
                ButtonTransText.Text = "Stop";

                RecognizedText recognizedText = await _textRecoClient.RecognizeTextAsync(_capturedFrame.ToSoftwareBitmap());
                TextRecoClient.RecognizedTextToBoxesAndTexts(recognizedText, out List<System.Drawing.Rectangle> boxes, out string[] texts);

                string targetLanguage = (string)ComboBoxLang.SelectedValue;
                var textList = new List<string>();
                foreach (var text in texts)
                {
                    textList.Add(await _textTransClient.TranslateText(text, targetLanguage));
                }
                string[] transTexts = [.. textList];

                Bitmap drawnBitmap = ImageUtils.ReplaceRectangleAndText(
                    _capturedFrame.ToBitmap(),
                    boxes,
                    transTexts);
                ImgCamera.Source = ImageFormatExtensions.Bitmap2ImageSource(drawnBitmap);

                _isTranslatingText = false;
                ButtonTransText.Text = "Translate";
                TextRecognized.Text = string.Join('\n', transTexts);
            }
        }

        private void ButtonClearClick(object sender, RoutedEventArgs e)
        {
            ImgCamera.Source = _blackMat.ToWriteableBitmap();
            _capturedFrame = _blackMat;
            TextRecognized.Text = string.Empty;

            ButtonRecognize.IsEnabled = false;
            ButtonOCR.IsEnabled = false;
            ButtonTrans.IsEnabled = false;
        }
    }
}
