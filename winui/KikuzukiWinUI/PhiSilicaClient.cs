using Microsoft.Extensions.AI;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kikuzuki
{
    public class PhiSilicaClient
    {
        public enum Status
        {
            Generating,
            End,
        }

        public class ProcessEventArgs : EventArgs
        {
            public string Output { get; set; } = string.Empty;
            public Status Status { get; set; } = Status.Generating;
        }

        public delegate void ProcessEventHandler(object mObjct, ProcessEventArgs mArgs);
        public event ProcessEventHandler? OnProcessed;
        
        private PhiSilica? chatClient;
        private CancellationTokenSource? _cts = null;

        public PhiSilicaClient()
        {
            InitAI();
        }

        private async void InitAI()
        {
            chatClient = await PhiSilica.CreateAsync();
        }

        public async Task ChatAsync(ChatMessage[] chatHistory)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (chatClient != null)
            {
                ChatResponse res = await chatClient.GetResponseAsync(
                        chatHistory,
                        null,
                        _cts.Token);

                OnProcessed?.Invoke(this, new ProcessEventArgs
                {
                    Status = Status.End,
                    Output = res.ToString(),
                });
            }
        }

        public async Task ChatStreamingAsync(ChatMessage[] chatHistory)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (chatClient != null)
            {
                await foreach (var messagePart in chatClient.GetStreamingResponseAsync(
                        chatHistory,
                        null,
                        _cts.Token))
                {
                    OnProcessed?.Invoke(this, new ProcessEventArgs
                    {
                        Status = Status.Generating,
                        Output = messagePart.ToString(),
                    });
                }

                OnProcessed?.Invoke(this, new ProcessEventArgs
                {
                    Status = Status.End,
                    Output = string.Empty,
                });
            }
        }

        public async Task<string> Chat(ChatMessage[] chatHistory)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (chatClient != null)
            {
                ChatResponse res = await chatClient.GetResponseAsync(
                        chatHistory,
                        null,
                        _cts.Token);
                return res.ToString();
            }
            return string.Empty;
        }

        public void StopGenerating()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}
