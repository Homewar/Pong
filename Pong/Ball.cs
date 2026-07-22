using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pong
{
    internal class Ball
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed;
        public int radius;

        public Ball(Vector2 position, Vector2 direction, float speed, int radius)
        {
            Position = position;
            Direction = direction;
            Speed = speed;
            this.radius = radius;
        }

        public void Move()
        {
            Position += Direction * Speed;
        }
    }
}
