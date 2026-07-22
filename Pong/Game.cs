using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Pong
{
    internal class Game
    {
        private Ball _ball;
        private Field _field;
        private Paddle _player1;
        private Paddle _player2;
        private Bitmap _bitmap;
        private Input _input;
        public Bitmap Bitmap => _bitmap;
        private NetworkClient _network;
        private GameState _lastState;
        private bool _hasState;
        private LocalPlayer _localPlayer;
        private RemotePlayer _remotePlayer;

        public Game(Input input)
        {
            _input = input;
            _field = new Field(800, 600);
            _bitmap = new Bitmap(_field.width, _field.height);
            _ball = new Ball(new System.Numerics.Vector2(_field.width / 2, _field.height / 2), new System.Numerics.Vector2(-1, 1), 7, 10);
            _network = new NetworkClient();
            _localPlayer = new LocalPlayer(_input, _network);
            _remotePlayer = new RemotePlayer(_network);
            _player1 = new Paddle(new System.Numerics.Vector2(50, _field.height / 2 - 50), 20, 100, 10, _field, _localPlayer);
            _player2 = new Paddle(new System.Numerics.Vector2(_field.width - 70, _field.height / 2 - 50), 20, 100, 10, _field, _remotePlayer);
        }

        public async Task Ready()
        {
            await _network.Connect();
            await _network.WaitForPlayer();
        }

        public void Run() 
        {
            if (!_network.IsPlayerConnected)
            {
                DrawWaiting();
                return;
            }

            // Только отправляем команду на сервер, НЕ двигаем ракетку локально
            _localPlayer.GetCommand(_player1);

            // Читаем все накопленные GameState, берём только последний (самый свежий)
            var state = _network.TryReceiveLatestGameState();
            if (state.HasValue)
            {
                _lastState = state.Value;
                _hasState = true;
            }

            // Применяем последнее полученное состояние от сервера
            if (_hasState)
            {
                ApplyServerState();
            }

            Draw();
        }

        private void ApplyServerState()
        {
            _ball.Position.X = _lastState.BallX;
            _ball.Position.Y = _lastState.BallY;
            _player1.position.Y = _lastState.Player1Y;
            _player2.position.Y = _lastState.Player2Y;
        }

        private void DrawWaiting()
        {
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.Clear(Color.Black);
                g.DrawString("Waiting for player...", new Font("Arial", 48, FontStyle.Bold), Brushes.White, new PointF(_field.width / 2 - 200, _field.height / 2 - 30));
            }
        }

        public void Draw() 
        {    
            using(Graphics g = Graphics.FromImage(_bitmap))
            {
                int lineWidth = 6;
                int dashHeight = 20;
                int gap = 20;
                int x = _field.width / 2 - lineWidth / 2;

                var _lightGray = new SolidBrush((Color)ColorTranslator.FromHtml("#1e1e1e"));
                g.Clear(Color.Black);
                g.DrawString($"{_lastState.Score1}", new Font("Arial", 280, FontStyle.Bold), _lightGray, new PointF(_field.width / 2 - 400, _field.height / 2 - 200));
                g.DrawString($"{_lastState.Score2}", new Font("Arial", 280, FontStyle.Bold), _lightGray, new PointF(_field.width / 2 + 100, _field.height / 2 - 200));
                for (int y = 0; y < _field.height; y += dashHeight + gap)
                {
                    g.FillRectangle(_lightGray, x, y, lineWidth, dashHeight);
                }
                g.FillEllipse(Brushes.White, _ball.Position.X - _ball.radius, _ball.Position.Y - _ball.radius, _ball.radius * 2, _ball.radius * 2);
                g.FillRectangle(Brushes.White, _player1.position.X, _player1.position.Y, _player1.width, _player1.height);
                g.FillRectangle(Brushes.White, _player2.position.X, _player2.position.Y, _player2.width, _player2.height);
            }
        }
    }
}