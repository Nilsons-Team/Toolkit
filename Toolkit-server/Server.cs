using System.Text;
using System.Net;
using System.Net.Sockets;

using Toolkit_Shared;
using Toolkit_Shared.Network.Packets;

namespace Toolkit_Server
{
    public class Server
    {
        public static readonly int DEFAULT_PORT = 1337;

        protected Logger logger;
        protected TcpListener server;
        protected List<TcpClient> clients;

        protected string[] args;
        protected byte[] buffer;

        protected bool acceptNewConnections = true;
        protected bool acceptNewPackets = true;
        protected int bytesPerPacket = 512;

        public int port;

        public Server(string[] args)
        {
            this.args = args;
            logger = new Logger("log.txt");
            clients = new List<TcpClient>();
            buffer = new byte[bytesPerPacket];
            
            port = DEFAULT_PORT;
            server = new TcpListener(IPAddress.Any, port);
        }

        private void ListenForConnections()
        {
            logger.Info("Listening for new connections.");
            while (acceptNewConnections)
            {
                TcpClient client = server.AcceptTcpClient();
                if (client.Connected)
                {
                    logger.Info($"Accepted connection from client '{client.Client.RemoteEndPoint}'.");
                    clients.Add(client);
                    Thread packetListener = new Thread(() => ListenForPackets(client));
                    packetListener.Name = $"Packet Listener #{packetListener.ManagedThreadId}";
                    packetListener.Start();
                }
            }
        }

        private void ListenForPackets(TcpClient client)
        {
            logger.Info($"Listening for packets from '{client.Client.RemoteEndPoint}'.");
            while (acceptNewPackets)
            {
                if (!client.Connected)
                {
                    logger.Info($"Client '{client.Client.RemoteEndPoint}' has disconnected.");
                    clients.Remove(client);
                    client.Close();
                    break;
                }

                NetworkStream network = client.GetStream();
                
                try
                {
                    int totalReadBytes = 0;
                    int readBytes;
                    readBytes = 0;
                    
                    // Make sure to handle packets of any size.
                    readBytes = network.Read(buffer, 0, buffer.Length);
                    totalReadBytes += readBytes;

                    var memory = new MemoryStream(buffer);
                    var binary = new BinaryReader(memory, Encoding.UTF8);
                    PacketType packetType = (PacketType) binary.ReadInt32();

                    switch (packetType)
                    {
                        case PacketType.APP_PACKET:
                            AppPacket app;
                            app.Id = binary.ReadInt64();
                            app.Name = binary.ReadString();
                            app.DeveloperCompanyId = binary.ReadInt64();
                            app.PublisherCompanyId = binary.ReadInt64();
                            app.UploadDatetime = binary.ReadString();
                            app.ReleaseDatetime = binary.ReadString();
                            app.DiscountPercent = binary.ReadInt64();
                            app.DiscountStartDatetime = binary.ReadString();
                            app.DiscountExpireDatetime = binary.ReadString();
                            app.AppTypeId = binary.ReadInt64();
                            app.AppReleaseStateId = binary.ReadInt64();
                            break;
                        case PacketType.USER_AUTH_PACKET:
                            UserAuthPacket auth;
                            auth.packetType = packetType;
                            auth.Login = binary.ReadString();
                            auth.Password = binary.ReadString();
                            logger.Info($"Received packet '{packetType}' ({auth}) from '{client.Client.RemoteEndPoint}'.");
                            break;
                        default:
                            break;
                    }
                }
                catch (IOException e)
                {
                    if (e.InnerException == null || e.InnerException is not SocketException)
                        return;

                    SocketException se = (SocketException) e.InnerException;
                    if (se.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        logger.Info($"Client '{client.Client.RemoteEndPoint}' closed connection.");
                    }
                }

                Thread.Sleep(50);
            }
        }
        public static void ProcessCurrentClientCommand(TcpClient client)
        {

        }

        public void Start()
        {
            logger.Info($"Server has been started ({DateTime.Now})." +
                         "\n\t\t\t--------------------" +
                         "\n\t\t\t   Toolkit Server   " +
                         "\n\t\t\t--------------------");

            server.Start();

            var connectionListener = new Thread(ListenForConnections);
            connectionListener.Name = $"Connection Listener #{connectionListener.ManagedThreadId}";
            connectionListener.Start();
        }

        public void Shutdown()
        {
            acceptNewConnections = false;
            acceptNewPackets = false;

            foreach (var client in clients)
            {
                client.Close();
            }
        }
    }
}