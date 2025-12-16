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
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
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
        private static readonly SoftwareBitmap blackBitmap = new(
            BitmapPixelFormat.Bgra8,
            600, // width
            480, // height,
            BitmapAlphaMode.Premultiplied);
        private readonly SoftwareBitmapSource blackbitmapSource = new();

        private string _selCameraId;
        private WinCamera? _cam;
        private MediaCaptureInitializationSettings? settings;
        private bool _isStreaming = false;
        private SoftwareBitmap _capturedImage = blackBitmap;

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

            this.AppWindow.Resize(new SizeInt32(755, 745));
        }

        private void InitializeDevice()
        {
            foreach (var camera in CameraEnumerator.Cameras)
            {
                ComboBoxCamera.Items.Add(camera.Name);
            }
        }

        private async void InitializeFunction()
        {
            await blackbitmapSource.SetBitmapAsync(blackBitmap);

            _imgDescClient = new ImageDescClient();
            _imgDescClient.OnProcessed += (mObjct, mArgs) =>
            {
                if (mArgs.Status == ImageDescClient.Status.End)
                {
                    _isRecognizing = false;
                    _ = DispatcherQueue.TryEnqueue(() =>
                    {
                        ButtonRecognizeText.Text = "Describe";
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

        private async void ButtonOpenClick(object _, RoutedEventArgs __)
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
            _capturedImage = convertedImage;

            ButtonRecognize.IsEnabled = true;
            ButtonOCR.IsEnabled = true;
            ButtonTrans.IsEnabled = true;
            TextRecognized.Text = string.Empty;
        }

        private async void ButtonCopyClick(object _, RoutedEventArgs __)
        {
            var package = Clipboard.GetContent();
            if (package.Contains(StandardDataFormats.Bitmap))
            {
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
                    catch (Exception)
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

        private async void ButtonCameraClick(object _, RoutedEventArgs __)
        {
            if (_isStreaming)
            {
                if (_capturedImage != null)
                {
                    _isStreaming = false;
                    ButtonCameraText.Text = "Camera Restart";

                    ButtonRecognize.IsEnabled = true;
                    ButtonOCR.IsEnabled = true;
                    ButtonTrans.IsEnabled = true;
                    TextRecognized.Text = string.Empty;

                    _cam?.PausePreview();
                    _cam?.ReleaseCamera();
                }
            }
            else
            {
                _isStreaming = true;
                ButtonCameraText.Text = "Capture";
                _capturedImage = blackBitmap;

                settings = new MediaCaptureInitializationSettings
                {
                    MemoryPreference = MediaCaptureMemoryPreference.Auto,
                    SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    VideoDeviceId = CameraEnumerator.Cameras[ComboBoxCamera.SelectedIndex].Id
                };

                _cam = new WinCamera(settings);
                await _cam.InitializeAsync();

                PreviewElement.SetMediaPlayer(_cam.MediaPlayer);
                _cam.StartPreview();
            }
        }

        private void ButtonRecognizeClick(object _, RoutedEventArgs __)
        {
            if (_imgDescClient == null) throw new Exception("Image Description Client not initialized.");
            if (_capturedImage == null) throw new Exception("No captured image available.");

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
                _ = _imgDescClient.DescribeImage(_capturedImage);
            }
        }

        private void ButtonOCRClick(object _, RoutedEventArgs __)
        {
            if (_textRecoClient == null) throw new Exception("Text Recognition Client not initialized.");
            if (_capturedImage == null) throw new Exception("No captured image available.");

            if (_isRecognizingText)
            {
                _isRecognizingText = false;
                ButtonOCRText.Text = "Recognize";
            }
            else
            {
                _isRecognizingText = true;
                ButtonOCRText.Text = "Stop";
                
                _ = RecognizeTextAndUpdateUI();
            }
        }

        private async Task RecognizeTextAndUpdateUI()
        {
            if (_textRecoClient == null) throw new Exception("Text Recognition Client not initialized.");
            if (_capturedImage == null) throw new Exception("No captured image available.");

            _isRecognizingText = true;
            ButtonOCRText.Text = "Stop";

            RecognizedText recognizedText = await _textRecoClient.RecognizeTextAsync(_capturedImage);
            TextRecoClient.RecognizedTextToBoxesAndTexts(recognizedText, out List<System.Drawing.Rectangle> boxes, out string[] texts);

            Bitmap drawnBitmap = ImageUtils.DrawRectangleAndText(
                ImageFormatExtensions.SoftwareBitmapToBitmap(_capturedImage),
                boxes,
                texts);
            ImgCamera.Source = ImageFormatExtensions.Bitmap2ImageSource(drawnBitmap);

            _isRecognizingText = false;
            ButtonOCRText.Text = "Recognize";
            TextRecognized.Text = string.Join('\n', texts);
        }

        private void ButtonTransClick(object _, RoutedEventArgs __)
        {
            if (_textRecoClient == null) throw new Exception("Text Recognition Client not initialized.");
            if (_textTransClient == null) throw new Exception("Text Translation Client not initialized.");
            if (_capturedImage == null) throw new Exception("No captured image available.");
            if (_isTranslatingText)
            {
                _isTranslatingText = false;
                ButtonTransText.Text = "Translate";
            }
            else
            {
                _isTranslatingText = true;
                ButtonTransText.Text = "Stop";

                _ = TranslateTextAndUpdateUI();
            }
        }

        private async Task TranslateTextAndUpdateUI()
        {
            if (_textRecoClient == null) throw new Exception("Text Recognition Client not initialized.");
            if (_textTransClient == null) throw new Exception("Text Translation Client not initialized.");
            if (_capturedImage == null) throw new Exception("No captured image available.");

            RecognizedText recognizedText = await _textRecoClient.RecognizeTextAsync(_capturedImage);
            TextRecoClient.RecognizedTextToBoxesAndTexts(recognizedText, out List<System.Drawing.Rectangle> boxes, out string[] texts);

            string targetLanguage = (string)ComboBoxLang.SelectedValue;
            var textList = new List<string>();
            foreach (var text in texts)
            {
                textList.Add(await _textTransClient.TranslateText(text, targetLanguage));
            }
            string[] transTexts = [.. textList];

            Bitmap drawnBitmap = ImageUtils.ReplaceRectangleAndText(
                ImageFormatExtensions.SoftwareBitmapToBitmap(_capturedImage),
                boxes,
                transTexts);
            ImgCamera.Source = ImageFormatExtensions.Bitmap2ImageSource(drawnBitmap);

            _isTranslatingText = false;
            ButtonTransText.Text = "Translate";
            TextRecognized.Text = string.Join('\n', transTexts);
        }

        private void ButtonClearClick(object _, RoutedEventArgs __)
        {
            _capturedImage = blackBitmap;
            ImgCamera.Source = blackbitmapSource;
            TextRecognized.Text = string.Empty;

            ButtonRecognize.IsEnabled = false;
            ButtonOCR.IsEnabled = false;
            ButtonTrans.IsEnabled = false;
        }
    }
}
