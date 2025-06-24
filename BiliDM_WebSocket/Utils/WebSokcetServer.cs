using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BiliDM_WebSocket.Utils
{
    public class WebSokcetServer : IDisposable
    {
        private HttpListener Listener { get; set; }
        private SynchronizedCollection<WebSocketAsync> Clients { get; set; } = new SynchronizedCollection<WebSocketAsync>();

        public void Start(string host, string port)
        {
            if (Listener is null || !Listener.IsListening)
            {
                try
                {
                    Close();
                    var listener = new HttpListener();
                    listener.Prefixes.Add($"http://{host}:{port}/");
                    listener.Start();
                    StartListener(listener);
                    Listener = listener;
                }
                catch (HttpListenerException ex)
                {
                    Close();
                    ENV.Log(ex.Message);
                }
                catch (Exception ex)
                {
                    Close();
                    ENV.Log(ex.Message);
                }
            }
        }

        public void Close()
        {
            Listener?.Close();
            Listener = null;
        }

        public void SendAllMessage(string message)
        {
            lock (Clients.SyncRoot)
            {
                foreach (var client in Clients)
                {
                    client.SendMessage(message);
                }
            }
        }

        private void StartListener(HttpListener httpListener)
        {
            Task.Run(() =>
            {
                while (httpListener.IsListening)
                {
                    try
                    {
                        var ctx = httpListener.GetContext();
                        if (ctx.Request.IsWebSocketRequest)
                        {
                            ListenWebSocketClient(ctx);
                        }
                        else
                        {
                            ctx.Response.StatusCode = 404;
                            ctx.Response.OutputStream.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        ENV.Log(ex.Message);
                    }
                }
            });
        }

        private async void ListenWebSocketClient(HttpListenerContext context)
        {
            var client = (await context.AcceptWebSocketAsync(null)).WebSocket;
            var clientAsync = new WebSocketAsync(client);
            Clients.Add(clientAsync);
            await clientAsync.Listening();
            Clients.Remove(clientAsync);
            client.Dispose();
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }

    public class WebSocketAsync : IDisposable
    {
        private bool IsDisposed { get; set; }
        private WebSocket WebSocket { get; }
        private SemaphoreSlim SendLock { get; } = new SemaphoreSlim(1, 1);

        public WebSocketAsync(WebSocket webSocket)
        {
            WebSocket = webSocket;
        }

        public async Task Listening(int buffeSize = 2048)
        {
            var buffer = new byte[buffeSize];
            while (WebSocket.State is WebSocketState.Open)
            {
                await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
        }

        public void SendMessage(string message)
        {
            var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            SendMessage(segment, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
        }

        public void SendMessage(ArraySegment<byte> messageData)
        {
            SendMessage(messageData, WebSocketMessageType.Binary, endOfMessage: true, CancellationToken.None);
        }

        private async void SendMessage(ArraySegment<byte> messageData, WebSocketMessageType messageType, bool endOfMessage, CancellationToken token)
        {
            await SendLock.WaitAsync(token);
            try
            {
                await WebSocket.SendAsync(messageData, messageType, endOfMessage, token);
            }
            catch { }
            finally
            {
                if (!IsDisposed)
                {
                    SendLock.Release();
                }
            }
        }

        public void Dispose()
        {
            IsDisposed = true;
            SendLock.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
