using Orleans;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace streamtest.api.Streaming
{
    public class EventMessageHandler : WebSocketHandler
    {
        public EventMessageHandler(WebSocketConnectionManager webSocketConnectionManager, IClusterClient clusterClient) : base(webSocketConnectionManager,clusterClient)
        {
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var socketId = WebSocketConnectionManager.GetId(socket);
            var message = $"{socketId} said: {Encoding.UTF8.GetString(buffer, 0, result.Count)}";

            await SendMessageToAllAsync(message);
        }

        public override async Task OnConnectedAsync(WebSocket socket)
        {
            await base.OnConnectedAsync(socket);


            // Register Channel


            var socketId = WebSocketConnectionManager.GetId(socket);
            await SendMessageToAllAsync($"{socketId} is now connected");
        }

        public override async Task OnDisconnectedAsync(WebSocket socket)
        {
            var socketId = WebSocketConnectionManager.GetId(socket);
            await base.OnDisconnectedAsync(socket);
            await SendMessageToAllAsync($"{socketId} is now disconnected");
        }
    }
}
