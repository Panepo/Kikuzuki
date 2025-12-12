using Microsoft.Graphics.Imaging;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        public static void RecognizedTextToBoxesAndTexts(
            RecognizedText recognizedText,
            out List<Rectangle> boxes,
            out string[] texts)
        {
            boxes = [];
            var textList = new List<string>();

            foreach (var line in recognizedText.Lines)
            {
                // Get bounding box points
                var bbox = line.BoundingBox;
                // Calculate min/max for rectangle
                int minX = (int)new[] { bbox.TopLeft.X, bbox.TopRight.X, bbox.BottomRight.X, bbox.BottomLeft.X }.Min();
                int minY = (int)new[] { bbox.TopLeft.Y, bbox.TopRight.Y, bbox.BottomRight.Y, bbox.BottomLeft.Y }.Min();
                int maxX = (int)new[] { bbox.TopLeft.X, bbox.TopRight.X, bbox.BottomRight.X, bbox.BottomLeft.X }.Max();
                int maxY = (int)new[] { bbox.TopLeft.Y, bbox.TopRight.Y, bbox.BottomRight.Y, bbox.BottomLeft.Y }.Max();

                boxes.Add(new Rectangle(minX, minY, maxX - minX, maxY - minY));
                textList.Add(line.Text);
            }

            texts = [.. textList];
        }
    }
}
