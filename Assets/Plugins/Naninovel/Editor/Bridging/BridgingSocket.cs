// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using Naninovel.Bridging;
using Naninovel.WebSocketSharp;
using Naninovel.WebSocketSharp.Server;

namespace Naninovel
{
    public class BridgingSocket : WebSocketBehavior, IWebSocket
    {
        public bool Open => ConnectionState == WebSocketState.Open;

        private readonly AsyncQueue<string> messages = new AsyncQueue<string>();

        public async Task<string> WaitMessageAsync (CancellationToken token)
        {
            return await messages.WaitAsync(token);
        }

        public async Task SendMessageAsync (string message, CancellationToken token)
        {
            if (!Open) return;
            await Task.Run(() => Send(message), token);
            await UniTask.SwitchToMainThread();
        }

        public async Task CloseAsync (CancellationToken token = default)
        {
            await Task.Run(() => CloseAsync(CloseStatusCode.Normal, ""), token);
            await UniTask.SwitchToMainThread();
        }

        public void Dispose ()
        {
            messages.Dispose();
        }

        protected override void OnMessage (MessageEventArgs e)
        {
            if (e.IsText && !string.IsNullOrWhiteSpace(e.Data))
                messages.Enqueue(e.Data);
        }
    }
}
