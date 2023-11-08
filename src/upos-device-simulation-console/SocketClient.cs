using System;
using System.Threading.Tasks;

namespace UposDeviceSimulationConsole
{
    public class SocketClient
    {

        readonly public SocketIOClient.SocketIO client;
        readonly SocketIOClient.SocketIOOptions _socketIOOptions;
        public SocketClient(SocketIOClient.SocketIO client, SocketIOClient.SocketIOOptions socketIOOptions)
        {
            this.client = client;
            _socketIOOptions = socketIOOptions;
        }
        public async Task Connect()
        {
            _socketIOOptions.ConnectionTimeout = new TimeSpan(1000);
            _socketIOOptions.Transport = SocketIOClient.Transport.TransportProtocol.WebSocket;
            await client.ConnectAsync();
        }

        public void EmitEvent(string eventName, object data)
        {
            client.EmitAsync(eventName, data);
        }

        public void Disconnect()
        {
            client.DisconnectAsync();
        }
    }
}
