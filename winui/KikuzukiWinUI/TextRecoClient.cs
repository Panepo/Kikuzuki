using KikuzukiWinUI;
using Microsoft.Graphics.Imaging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Kikuzuki
{
    public class TextRecoClient
    {
        private TextRecognizer? _textRecognizer;

        public TextRecoClient()
        {
            InitTextRecognizer();
        }

        private static async void InitTextRecognizer()
        {
            var readyState = TextRecognizer.GetReadyState();
            if (readyState is AIFeatureReadyState.Ready or AIFeatureReadyState.NotReady)
            {
                if (readyState == AIFeatureReadyState.NotReady)
                {
                    var operation = await TextRecognizer.EnsureReadyAsync();

                    if (operation.Status != AIFeatureReadyResultState.Success)
                    {
                        throw new Exception("Text Recognition is not available.");
                    }
                }
            }
            else
            {
                var msg = readyState == AIFeatureReadyState.DisabledByUser
                    ? "Disabled by user."
                    : "Not supported on this system.";
                throw new Exception($"Text Recognition is not available: {msg}");
            }
        }

        public async Task<RecognizedText> RecognizeTextAsync(SoftwareBitmap bitmap)
        {
            using var imageBuffer = ImageBuffer.CreateForSoftwareBitmap(bitmap);
            _textRecognizer ??= await TextRecognizer.CreateAsync();
            RecognizedText? result = _textRecognizer.RecognizeTextFromImage(imageBuffer);
            return result ?? RecognizedText.FromAbi(IntPtr.Zero);
        }
    }
}
