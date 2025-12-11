using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kikuzuki
{
    internal class Camera
    {
        // =================================================================================
        // global variables
        // =================================================================================
        private VideoCapture _capture;
        private DispatcherTimer _timer;

        private int _height;
        private int _width;
        private double _brightness;
        private double _sharpness;
        private double _contrast;

        // =================================================================================
        // constructor
        // =================================================================================
        public Camera(int deviceId, EventHandler<object> eventHandler)
        {
            _capture = new VideoCapture(deviceId);
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(33) // 約30fps
            };
            _timer.Tick += eventHandler;
        }

        // =================================================================================
        // timer controller
        // =================================================================================
        public void StopTimer()
        {
            if (_timer.IsEnabled)
                _timer.Stop();
        }

        public void StartTimer()
        {
            if (!_timer.IsEnabled)
                _timer.Start();
        }

        // =================================================================================
        // capture related function
        // =================================================================================
        public Mat FrameCapture()
        {
            if (_capture != null)
            {
                var mat = new Mat();
                if (_capture.Read(mat))
                    return mat;
                else
                    throw new Exception("Failed to read frame from camera.");
            }
            else
            {
                throw new Exception("Camera is not initialized.");
            }
        }

        // =================================================================================
        // parameter settings
        // =================================================================================
        public void SetDevice(int deviceId, EventHandler<object> eventHandler)
        {
            _capture?.Dispose();
            _capture = new VideoCapture(deviceId);
            //capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
            //capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);

            _timer = new DispatcherTimer();
            _timer.Tick += eventHandler;
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        // =================================================================================
        // dispose
        // =================================================================================
        public void ReleaseCamera()
        {
            _capture?.Dispose();
        }
    }
}
