using System.Numerics;

namespace Pong
{
    internal class Bot : IController
    {
        private readonly Ball _ball;
        private readonly Field _field;


        private const float DeadZone = 10f;

        public Bot(Field field, Ball ball)
        {
            _ball = ball;
            _field = field;
        }

        private PaddleCommand MoveToTarget(float targetY, Paddle paddle)
        {
            float paddleCenter = paddle.position.Y + paddle.height / 2f;
            float delta = targetY - paddleCenter;

            if (delta > DeadZone)
                return PaddleCommand.Down;

            if (delta < -DeadZone)
                return PaddleCommand.Up;

            return PaddleCommand.None;
        }

        private PaddleCommand MoveToCenter(Paddle paddle)
        {
            float fieldCenterY = _field.height / 2f;
            return MoveToTarget(fieldCenterY, paddle);
        }

        public PaddleCommand GetCommand(Paddle paddle)
        {
            // Если мяч летит от бота — возвращаемся в центр.
            if (_ball.Direction.X < 0)
            {
                MoveToCenter(paddle);
            }
            float ballCenter = _ball.Position.Y + _ball.radius / 2f;
            return MoveToTarget(ballCenter, paddle);
        }
    }
}