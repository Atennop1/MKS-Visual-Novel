// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using Naninovel.Bridging;
using Naninovel.WebSocketSharp.Server;

namespace Naninovel
{
    public class BridgingListener : IConnectionListener
    {
        public bool Listening => server?.IsListening ?? false;

        private readonly AsyncQueue<BridgingSocket> sockets = new AsyncQueue<BridgingSocket>();
        private WebSocketServer server;
        private CancellationTokenSource cts;

        public void ListenLocalHost (int port)
        {
            StopListening();
            cts = new CancellationTokenSource();
            server = new WebSocketServer(port);
            server.Start();
            server.AddWebSocketService<BridgingSocket>("/", sockets.Enqueue);
        }

        public void StopListening ()
        {
            server?.Stop();
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }

        public async Task<IWebSocket> WaitConnectionAsync ()
        {
            return await sockets.WaitAsync(cts.Token);
        }

        public void Dispose ()
        {
            server?.Stop();
            cts?.Dispose();
            sockets.Dispose();
        }
    }
}
