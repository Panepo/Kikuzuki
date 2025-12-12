using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Kikuzuki
{
    public class TextTransClient
    {
        public readonly List<string> languages =
        [
            "Afrikaans",
            "Arabic",
            "Czech",
            "Danish",
            "Dutch",
            "English",
            "Filipino",
            "Finnish",
            "French",
            "German",
            "Greek",
            "Hindi",
            "Indonesian",
            "Italian",
            "Japanese",
            "Korean",
            "Mandarin",
            "Polish",
            "Portuguese",
            "Romanian",
            "Russian",
            "Serbian",
            "Slovak",
            "Spanish",
            "Thai",
            "Turkish",
            "Vietnamese"
        ];

        private PhiSilica? chatClient;
        private CancellationTokenSource? _cts = null;

        public TextTransClient()
        {
            InitAI();
        }

        private async void InitAI()
        {
            chatClient = await PhiSilica.CreateAsync();
        }

        public Task<string> TranslateText(string text, string language)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            if (chatClient != null)
            {
                var chatHistory = new ChatMessage[]
                {
                    new(ChatRole.System, "You translate user provided text. Do not reply with any extraneous content besides the translated text itself."),
                    new(ChatRole.User, $@"Translate the following text to {language}: '{text}'")
                };
                return chatClient.GetResponseAsync(
                        chatHistory,
                        null,
                        _cts.Token).ContinueWith(t => t.Result.ToString());
            }
            else
            {
                return Task.FromResult(string.Empty);
            }
        }

        public void StopGenerating()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}
