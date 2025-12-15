using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace Kikuzuki
{
    public class WinCamera
    {
        private MediaCapture? _capture;
        private DispatcherTimer _timer;

        public WinCamera(MediaCaptureInitializationSettings settings, EventHandler<object> eventHandler)
        {
            _ = InitializeAsync(settings);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(33) // 約30fps
            };
            _timer.Tick += eventHandler;
        }

        public async Task InitializeAsync(MediaCaptureInitializationSettings settings)
        {
            _capture = new MediaCapture();
            await _capture.InitializeAsync(settings); // You can pass MediaCaptureInitializationSettings to pick device
        }

        // =================================================================================
        // timer controller
        // =================================================================================
        public async void StopTimer()
        {
            if (_timer.IsEnabled && _capture != null)
            {
                await _capture.StopPreviewAsync();
                _timer.Stop();
            }    
        }

        public async void StartTimer()
        {
            if (!_timer.IsEnabled && _capture != null)
            {
                await _capture.StartPreviewAsync();
                _timer.Start();
            }
        }

        // =================================================================================
        // capture related function
        // =================================================================================
        public async Task<SoftwareBitmap?> FrameCaptureAsync()
        {
            if (_capture == null)
                return null;

            // Create an in-memory stream to store the captured photo
            using var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();

            // Capture the current frame as a JPEG image
            await _capture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

            // Reset stream position to the beginning
            stream.Seek(0);

            // Decode the image to a SoftwareBitmap in BGRA8 format
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            return softwareBitmap;
        }

        // =================================================================================
        // parameter settings
        // =================================================================================
        public async void SetDevice(MediaCaptureInitializationSettings settings, EventHandler<object> eventHandler)
        {
            _capture?.Dispose();
            await InitializeAsync(settings);

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
