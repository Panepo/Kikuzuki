using Microsoft.Graphics.Imaging;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.ContentSafety;
using Microsoft.Windows.AI.Imaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Kikuzuki
{
    public class ImageDescClient
    {
        public enum Status
        {
            Generating,
            End,
            Error
        }

        public class ProcessEventArgs : EventArgs
        {
            public string Output { get; set; } = string.Empty;
            public Status Status { get; set; } = Status.Generating;
        }

        public delegate void ProcessEventHandler(object mObjct, ProcessEventArgs mArgs);
        public event ProcessEventHandler? OnProcessed;

        private readonly Dictionary<string, ImageDescriptionKind> _descriptionKindDictionary = new Dictionary<string, ImageDescriptionKind>
        {
            { "Accessible", ImageDescriptionKind.AccessibleDescription },
            { "Caption", ImageDescriptionKind.BriefDescription },
            { "Detailed", ImageDescriptionKind.DetailedDescription },
            { "OfficeCharts", ImageDescriptionKind.DiagramDescription },
        };

        private ImageDescriptionGenerator? _imageDescriptor;
        private ImageDescriptionKind _currentKind = ImageDescriptionKind.BriefDescription;
        private CancellationTokenSource? _cts;

        public ImageDescClient()
        {
            InitImageDescriptor();
        }

        private static async void InitImageDescriptor()
        {
            var readyState = ImageDescriptionGenerator.GetReadyState();
            if (readyState is AIFeatureReadyState.Ready or AIFeatureReadyState.NotReady)
            {
                if (readyState == AIFeatureReadyState.NotReady)
                {
                    var operation = await ImageDescriptionGenerator.EnsureReadyAsync();

                    if (operation.Status != AIFeatureReadyResultState.Success)
                    {
                        throw new Exception("Image Description is not available");
                    }
                }
            }
            else
            {
                var msg = readyState == AIFeatureReadyState.DisabledByUser
                    ? "Disabled by user."
                    : "Not supported on this system.";
                throw new Exception("Image Description is not available");
            }

        }

        public void SetDescriptionKind(string kind)
        {
            if (_descriptionKindDictionary.ContainsKey(kind))
            {
                _currentKind = _descriptionKindDictionary[kind];
            }
        }

        public void StopDescribing()
        {
            _cts?.Cancel();
        }

        public async Task DescribeImage(SoftwareBitmap bitmap)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                using var bitmapBuffer = ImageBuffer.CreateForSoftwareBitmap(bitmap);
                _imageDescriptor ??= await ImageDescriptionGenerator.CreateAsync();
                var describeTask = _imageDescriptor.DescribeAsync(bitmapBuffer, _currentKind, new ContentFilterOptions());

                if (describeTask != null)
                {
                    describeTask.Progress += (asyncInfo, delta) =>
                    {
                        OnProcessed?.Invoke(this, new ProcessEventArgs
                        {
                            Status = Status.Generating,
                            Output = delta,
                        });

                        if (_cts.IsCancellationRequested == true)
                        {
                            describeTask.Cancel();
                        }
                    };

                    var response = await describeTask.AsTask(_cts.Token);

                    OnProcessed?.Invoke(this, new ProcessEventArgs
                    {
                        Status = Status.End,
                        Output = response.Description,
                    });
                }
            }
            catch (TaskCanceledException)
            {
                // Don't do anything
            }
            catch (Exception ex)
            {
                throw new Exception("Error during image description: " + ex.Message);
            }
        }
    }
}
