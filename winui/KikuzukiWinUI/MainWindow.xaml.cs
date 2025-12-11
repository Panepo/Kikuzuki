using Kikuzuki;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.AI.Imaging;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using Windows.Graphics;
using Windows.Graphics.Imaging;
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
        
        private bool _isCaptured = false;
        private Mat _capturedFrame = _blackMat;

        private ImageDescClient? _imgDescClient;
        private bool _isRecognizing = false;

        private TextRecoClient? _textRecoClient;
        private bool _isRecognizingText = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDevice();
            InitializeFunction();

            ImgCamera.Source = _frame.ToWriteableBitmap();

            this.AppWindow.Resize(new SizeInt32(610, 745));
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
                    ButtonRecognizeText.Text = string.Empty;

                    _isCaptured = true;
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

                _isCaptured = false;
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
                ButtonRecognizeText.Text = "Recognize";
            }
            else
            {
                _isRecognizingText = true;
                ButtonRecognizeText.Text = "Stop";
                RecognizedText recognizedText = await _textRecoClient.RecognizeTextAsync(_capturedFrame.ToSoftwareBitmap());

                _isRecognizingText = false;
                RenderRecognizedText(recognizedText);
                ButtonRecognizeText.Text = "Recognize";
            }
        }

        private void RenderRecognizedText(RecognizedText recognizedText)
        {
            List<string> lines = new();

            foreach (var line in recognizedText.Lines)
            {
                lines.Add(line.Text);

                SolidColorBrush backgroundBrush = new SolidColorBrush
                {
                    Color = Colors.Black,
                    Opacity = .6
                };

                Grid grid = new Grid
                {
                    Background = backgroundBrush,
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(4, 3, 4, 4)
                };

                try
                {
                    var height = Math.Abs((int)line.BoundingBox.TopRight.Y - (int)line.BoundingBox.BottomRight.Y) * .85;
                    TextBlock block = new TextBlock
                    {
                        IsTextSelectionEnabled = true,
                        Foreground = new SolidColorBrush(Colors.White),
                        Text = line.Text,
                        FontSize = height > 0 ? height : 1,
                    };

                    grid.Children.Add(block);
                    RectCanvas.Children.Add(grid);
                    Canvas.SetLeft(grid, line.BoundingBox.TopLeft.X);
                    Canvas.SetTop(grid, line.BoundingBox.TopLeft.Y);
                }
                catch
                {
                    continue;
                }
            }

            TextRecognized.Text = string.Join('\n', lines);
        }

        private async void ButtonClearClick(object sender, RoutedEventArgs e)
        {
            ImgCamera.Source = _blackMat.ToWriteableBitmap();
            _capturedFrame = _blackMat;
            TextRecognized.Text = string.Empty;
        }
    }
}
