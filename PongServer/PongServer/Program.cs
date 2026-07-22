using Server;
using System.Net;
using System.Net.Sockets;

namespace Server
{ 
    class Program
    {
        public enum PaddleCommand
        {
            Up = 1,
            Down = 2,
            None = 0
        }

        private const byte PlayerReadyPacket = 0xFF;

        public static async Task Main()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 7777);
            server.Start();
            Console.WriteLine("Server started on 127.0.0.1:7777");

            while (true)
            {
                try
                {
                    Console.WriteLine("\nWaiting for players...");

                    Console.WriteLine("Waiting for Player1...");
                    TcpClient client1 = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Player1 connected!");

                    Console.WriteLine("Waiting for Player2...");
                    TcpClient client2 = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Player2 connected!");

                    NetworkStream stream1 = client1.GetStream();
                    NetworkStream stream2 = client2.GetStream();

                    await SendPlayerReady(stream1);
                    await SendPlayerReady(stream2);

                    Console.WriteLine("Game started!");

                    var game = new GameServer(stream1, stream2);

                    // Играем, пока оба клиента подключены
                    while (client1.Connected && client2.Connected)
                    {
                        await game.Update();
                    }

                    Console.WriteLine("Players disconnected. Cleaning up...");

                    client1.Close();
                    client2.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private static async Task SendPlayerReady(NetworkStream stream)
        {
            if (stream == null)
                return;

            await stream.WriteAsync(new byte[] { PlayerReadyPacket });
        }
    }
}