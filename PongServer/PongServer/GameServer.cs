using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Server.Program;

namespace Server
{
    public struct GameState
    {
        public float BallX { get; set; }
        public float BallY { get; set; }

        public float Player1Y { get; set; }
        public float Player2Y { get; set; }

        public byte Score1 { get; set; }
        public byte Score2 { get; set; }

        public byte[] ToBytes()
        {
            using (var ms = new MemoryStream(18))
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(BallX);
                writer.Write(BallY);
                writer.Write(Player1Y);
                writer.Write(Player2Y);
                writer.Write(Score1);
                writer.Write(Score2);
                return ms.ToArray();
            }
        }

        public static GameState FromBytes(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                return new GameState
                {
                    BallX = reader.ReadSingle(),
                    BallY = reader.ReadSingle(),
                    Player1Y = reader.ReadSingle(),
                    Player2Y = reader.ReadSingle(),
                    Score1 = reader.ReadByte(),
                    Score2 = reader.ReadByte()
                };
            }
        }
    }

    internal class GameServer
    {
        private Paddle _player1;
        private Paddle _player2;
        private Ball _ball;
        private Field _field;
        private int _scorePlayer1;
        private int _scorePlayer2;
        private GameState _state;

        private NetworkStream _stream1;
        private NetworkStream _stream2;

        private const byte HeaderGameState = 0x01;

        public GameServer(NetworkStream stream1, NetworkStream stream2)
        {
            _stream1 = stream1;
            _stream2 = stream2;

            _field = new Field(800, 600);
            _ball = new Ball(new System.Numerics.Vector2(_field.width / 2, _field.height / 2), new System.Numerics.Vector2(-1, 1), 7, 10);
            _player1 = new Paddle(new System.Numerics.Vector2(50, _field.height / 2 - 50), 20, 100, 10, _field);
            _player2 = new Paddle(new System.Numerics.Vector2(_field.width - 70, _field.height / 2 - 50), 20, 100, 10, _field);
        }

        public async Task Update()
        {
            // Читаем команды синхронно
            ReadCommand(_stream1, cmd => _player1.Update(cmd));
            ReadCommand(_stream2, cmd => _player2.Update(cmd));

            _ball.Move();
            Physics();

            // Отправляем состояние параллельно обоим клиентам
            await Task.WhenAll(
                SendStateToClient(_stream1),
                SendStateToClient(_stream2)
            );

            // Ждём ~60 FPS
            await Task.Delay(16);
        }

        private void ReadCommand(NetworkStream stream, Action<PaddleCommand> applyCommand)
        {
            try
            {
                // Читаем ВСЕ накопленные команды, применяем только последнюю
                int lastCmd = -1;
                while (stream.DataAvailable)
                {
                    int b = stream.ReadByte();
                    if (b != -1)
                        lastCmd = b;
                }
                if (lastCmd != -1)
                {
                    applyCommand((PaddleCommand)lastCmd);
                }
            }
            catch
            {
            }
        }

        private async Task SendStateToClient(NetworkStream stream)
        {
            if (_ball == null) return;

            _state.BallX = _ball.Position.X;
            _state.BallY = _ball.Position.Y;
            _state.Player1Y = _player1.position.Y;
            _state.Player2Y = _player2.position.Y;
            _state.Score1 = (byte)_scorePlayer1;
            _state.Score2 = (byte)_scorePlayer2;

            byte[] stateBytes = _state.ToBytes();
            byte[] packet = new byte[1 + stateBytes.Length];
            packet[0] = HeaderGameState;
            Buffer.BlockCopy(stateBytes, 0, packet, 1, stateBytes.Length);

            try
            {
                await stream.WriteAsync(packet, 0, packet.Length);
            }
            catch
            {
            }
        }

        public void Physics()
        {
            // Отскок от верхней/нижней границы поля
            if (_ball.Position.Y - _ball.radius <= 0 || _ball.Position.Y + _ball.radius >= _field.height)
            {
                _ball.Direction.Y *= -1;
            }

            // Отскок от левой ракетки (_player1)
            if (_ball.Direction.X < 0 &&
                _ball.Position.X - _ball.radius <= _player1.position.X + _player1.width &&
                _ball.Position.X + _ball.radius >= _player1.position.X &&
                _ball.Position.Y + _ball.radius >= _player1.position.Y &&
                _ball.Position.Y - _ball.radius <= _player1.position.Y + _player1.height)
            {
                _ball.Direction.X *= -1;
            }

            // Отскок от правой ракетки (_player2)
            if (_ball.Direction.X > 0 &&
                _ball.Position.X + _ball.radius >= _player2.position.X &&
                _ball.Position.X - _ball.radius <= _player2.position.X + _player2.width &&
                _ball.Position.Y + _ball.radius >= _player2.position.Y &&
                _ball.Position.Y - _ball.radius <= _player2.position.Y + _player2.height)
            {
                _ball.Direction.X *= -1;
            }

            // Гол в левые ворота
            if (_ball.Position.X - _ball.radius <= 0)
            {
                _ball.Position = new System.Numerics.Vector2(_field.width / 2, _field.height / 2);
                _scorePlayer2++;
                Random dirLeft = new Random();
                int randomDirectionLeft = dirLeft.Next(0, 2) == 0 ? -1 : 1;
                int randomDirectionYLeft = dirLeft.Next(0, 2) == 0 ? -1 : 1;
                _ball.Direction = new System.Numerics.Vector2(randomDirectionLeft, randomDirectionYLeft);
            }

            // Гол в правые ворота
            if (_ball.Position.X + _ball.radius >= _field.width)
            {
                _ball.Position = new System.Numerics.Vector2(_field.width / 2, _field.height / 2);
                _scorePlayer1++;
                Random dirRight = new Random();
                int randomDirectionRight = dirRight.Next(0, 2) == 0 ? -1 : 1;
                int randomDirectionYRight = dirRight.Next(0, 2) == 0 ? -1 : 1;
                _ball.Direction = new System.Numerics.Vector2(randomDirectionRight, randomDirectionYRight);
            }
        }
    }
}