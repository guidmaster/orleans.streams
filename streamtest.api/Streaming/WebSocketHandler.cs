using Orleans;
using Orleans.Streams;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace streamtest.api.Streaming
{
    public abstract class WebSocketHandler
    {

        private IAsyncStream<string> stream;
        private StreamSubscriptionHandle<string> subscription;
        private readonly IClusterClient _clusterClient;

        protected WebSocketConnectionManager WebSocketConnectionManager { get; set; }

        public WebSocketHandler(WebSocketConnectionManager webSocketConnectionManager, IClusterClient clusterClient)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
            this._clusterClient = clusterClient;
        }

        public virtual async Task OnConnectedAsync(WebSocket socket)
        {
            var streamProvider = _clusterClient.GetStreamProvider("myname");
            var streamId = Guid.Empty;
            var stream = streamProvider.GetStream<string>(streamId, "TIME");

            var handlers = await stream.GetAllSubscriptionHandles();

            if (null == handlers || 0 == handlers.Count)
            {
                subscription = await stream.SubscribeAsync(OnStreamMessage);
            }
            else
            {
                foreach (var handler in handlers)
                {
                    subscription = await handler.ResumeAsync(OnStreamMessage);
                }
            }

            WebSocketConnectionManager.AddSocket(socket);
        }

        private Task OnStreamMessage(string message, StreamSequenceToken sst)
        {
            return SendMessageToAllAsync(message);
        }

        public virtual async Task OnDisconnectedAsync(WebSocket socket)
        {
            await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket));
            //await subscription.UnsubscribeAsync();
        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                                                                  offset: 0,
                                                                  count: message.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageAsync(string socketId, string message)
        {
            await SendMessageAsync(WebSocketConnectionManager.GetSocketById(socketId), message);
        }

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var pair in WebSocketConnectionManager.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                    await SendMessageAsync(pair.Value, message);
            }
        }

        //TODO - decide if exposing the message string is better than exposing the result and buffer
        public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }

}
