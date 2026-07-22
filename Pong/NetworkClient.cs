using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Pong
{
    internal class NetworkClient
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private bool _playerConnected;

        public bool IsConnected => _stream != null;
        public bool IsPlayerConnected => _playerConnected;

        private const byte PlayerReadyPacket = 0xFF;
        private const byte HeaderGameState = 0x01;

        public async Task Connect()
        {
            try
            {
                _tcpClient = new TcpClient();
                var connectTask = _tcpClient.ConnectAsync("127.0.0.1", 7777);
                var timeoutTask = Task.Delay(5000);
                if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
                {
                    Debug.WriteLine("Connection timeout");
                    _tcpClient = null;
                    _stream = null;
                    return;
                }
                _stream = _tcpClient.GetStream();
                Debug.WriteLine("Connected to server");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to connect: {ex.Message}");
                _tcpClient = null;
                _stream = null;
            }
        }

        public void SendCommand(PaddleCommand command)
        {
            if (_stream == null)
                return;

            _stream.WriteByte((byte)command);
        }

        public async Task WaitForPlayer()
        {
            if (_stream == null)
                return;

            try
            {
                Debug.WriteLine("Waiting for second player...");
                var readTask = Task.Run(() => _stream.ReadByte());
                var timeoutTask = Task.Delay(30000);
                if (await Task.WhenAny(readTask, timeoutTask) == timeoutTask)
                {
                    Debug.WriteLine("Wait for player timeout");
                    return;
                }
                int b = await readTask;
                if (b == PlayerReadyPacket)
                {
                    _playerConnected = true;
                    Debug.WriteLine("Second player connected");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WaitForPlayer error: {ex.Message}");
            }
        }

        private GameState? ReadOneGameState()
        {
            try
            {
                // Читаем заголовок (1 байт)
                int header = _stream.ReadByte();
                if (header != HeaderGameState)
                    return null;

                // Читаем 18 байт GameState
                byte[] stateBytes = new byte[18];
                int totalRead = 0;
                while (totalRead < 18)
                {
                    int read = _stream.Read(stateBytes, totalRead, 18 - totalRead);
                    if (read == 0)
                        return null;
                    totalRead += read;
                }

                return GameState.FromBytes(stateBytes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Читает все накопленные GameState и возвращает только последний (самый свежий).
        /// </summary>
        public GameState? TryReceiveLatestGameState()
        {
            if (_stream == null || !_stream.DataAvailable)
                return null;

            GameState? latest = null;

            while (_stream.DataAvailable)
            {
                var state = ReadOneGameState();
                if (state.HasValue)
                    latest = state;
                else
                    break; // некорректные данные — выходим
            }

            return latest;
        }
    }
}