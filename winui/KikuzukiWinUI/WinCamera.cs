using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;

namespace Kikuzuki
{
    public class WinCamera(MediaCaptureInitializationSettings settings)
    {
        private MediaCapture? _mediaCapture;
        private MediaFrameSource? _frameSource;

        public MediaPlayer? MediaPlayer { get; private set; }
        private MediaCaptureInitializationSettings _settings = settings;

        public async Task InitializeAsync()
        {
            try
            {
                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync(_settings);

                _frameSource = null;

                var previewSource = _mediaCapture.FrameSources
                    .FirstOrDefault(kv =>
                        kv.Value.Info.MediaStreamType == MediaStreamType.VideoPreview &&
                        kv.Value.Info.SourceKind == MediaFrameSourceKind.Color)
                    .Value;

                if (previewSource != null)
                {
                    _frameSource = previewSource;
                }
                else
                {
                    var recordSource = _mediaCapture.FrameSources
                        .FirstOrDefault(kv =>
                            kv.Value.Info.MediaStreamType == MediaStreamType.VideoRecord &&
                            kv.Value.Info.SourceKind == MediaFrameSourceKind.Color)
                        .Value;
                    if (recordSource != null)
                        _frameSource = recordSource;
                }

                if (_frameSource == null)
                {
                    throw new Exception("No suitable camera frame source found.");
                }

                MediaPlayer = new MediaPlayer
                {
                    RealTimePlayback = true,
                    AutoPlay = false,
                    Source = MediaSource.CreateFromMediaFrameSource(_frameSource)
                };
            }
            catch (UnauthorizedAccessException)
            {
                _mediaCapture?.Dispose();
                throw new Exception("Camera access denied.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitializeAsync failed: {ex}");
                _mediaCapture?.Dispose();
                throw new Exception("Camera initialization failed.");
            }
        }

        // =================================================================================
        // timer controller
        // =================================================================================
        public void PausePreview()
        {
            MediaPlayer?.Pause();
        }

        public void StartPreview()
        {
            MediaPlayer?.Play();
        }

        // =================================================================================
        // capture related function
        // =================================================================================
        //public async Task<SoftwareBitmap?> FrameCaptureAsync()
        //{
        //    if (_capture == null)
        //        return null;

        //    // Create an in-memory stream to store the captured photo
        //    using var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();

        //    // Capture the current frame as a JPEG image
        //    await _capture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

        //    // Reset stream position to the beginning
        //    stream.Seek(0);

        //    // Decode the image to a SoftwareBitmap in BGRA8 format
        //    var decoder = await BitmapDecoder.CreateAsync(stream);
        //    var softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

        //    return softwareBitmap;
        //}

        // =================================================================================
        // dispose
        // =================================================================================
        public void ReleaseCamera()
        {
            _mediaCapture?.Dispose();
        }
    }
}
