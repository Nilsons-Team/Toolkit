using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Toolkit_Shared;
using Toolkit_Shared.Network.Packets;

namespace Toolkit_Client.Modules
{
    public class Client
    {
        protected Logger logger;
        
        public TcpClient client;
        public string serverIp;
        public int serverPort;

        public Client()
        {
            logger = new Logger("log.txt");
            client = new TcpClient();
        }

        public void Connect(string ip, int port)
        {
            try
            {
                client.Connect(ip, port);
                if (client.Connected)
                    logger.Info($"Connected to server '{ip}:{port}'.");
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    logger.Warning($"Could not connect to server '{ip}:{port}' because it refused connection." +
                                    "\n\t... Probably the port is incorrect or it is not open to connections.");
                }
            }
        }

        public void SendMessage(string message)
        {
            if (!client.Connected)
                return;

            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            var stream = client.GetStream();
            stream.Write(messageBytes, 0, messageBytes.Length);
            logger.Info($"Sent message to server: \"{message}\".");
        }

        public void SendPacket(PacketType type, object packet)
        {
            var memory = new MemoryStream();
            var binary = new BinaryWriter(memory, Encoding.UTF8);

            switch (type)
            {
                case PacketType.APP_PACKET:
                    AppPacket app = (AppPacket) packet;

                    binary.Write((int)type);
                    binary.Write(app.Id);
                    binary.Write(app.Name);
                    binary.Write(app.DeveloperCompanyId);
                    binary.Write(app.PublisherCompanyId);
                    binary.Write(app.UploadDatetime);
                    binary.Write(app.ReleaseDatetime);
                    binary.Write(app.DiscountPercent);
                    binary.Write(app.DiscountStartDatetime);
                    binary.Write(app.DiscountExpireDatetime);
                    binary.Write(app.AppTypeId);
                    binary.Write(app.AppReleaseStateId);
                    break;

                case PacketType.USER_AUTH_PACKET:
                    UserAuthPacket auth = (UserAuthPacket) packet;

                    binary.Write((int)auth.packetType);
                    binary.Write(auth.Login);
                    binary.Write(auth.Password);
                    break;

                default:
                    return;
            }

            var network = client.GetStream();
            memory.WriteTo(network);
            logger.Info($"Sent packet '{type}' ({memory.Length} bytes) to '{client.Client.RemoteEndPoint}'.");
        }
    }
}
