using Kikuzuki;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using OpenCvSharp;
using System;
using Windows.Graphics;
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
        private CameraDevice _selCamera;
        private Camera? _cam;
        private Mat? _frame;
        private bool _isStreaming = false;
        
        private bool _isCaptured = false;
        private Mat? _capturedFrame;

        private ImageDescClient? _imgDescClient;
        private bool _isRecognizing = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDevice();
            InitializeFunction();

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
                _cam = new Camera(_selCamera.deviceID, new EventHandler<object>(ProcessFrame));
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
        }

        private void ComboBoxCameraChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedCamera = (string)ComboBoxCamera.SelectedValue;

            for (var i = 0; i < CameraEnumerator.Cameras.Length; i++)
            {
                if (selectedCamera == CameraEnumerator.Cameras[i].deviceName)
                {
                    this._selCamera = CameraEnumerator.Cameras[i];
                    _cam = new Camera(CameraEnumerator.Cameras[i].deviceID, new EventHandler<object>(ProcessFrame));
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
                    _isCaptured = true;
                    ButtonCameraText.Text = "Camera Restart";

                    _capturedFrame = _frame.Clone();
                    ImageSource bmp = _frame.ToWriteableBitmap();
                    ImgCamera.Source = bmp;
                    _cam?.StopTimer();
                }
            }
            else
            {
                _isStreaming = true;
                _isCaptured = false;
                ButtonCameraText.Text = "Capture";
                _cam?.StartTimer();
            }
        }

        private void ButtonRecognizeClick(object sender, RoutedEventArgs e)
        {
            if (_isRecognizing)
            {
                _isRecognizing = false;
                ButtonRecognizeText.Text = "Recognize";
                _imgDescClient?.StopDescribing();
            }
            else
            {
                _isRecognizing = true;
                ButtonRecognizeText.Text = "Recognizing...";
            }
        }
    }
}
