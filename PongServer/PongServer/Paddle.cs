using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Server.Program;
namespace Server
{
    internal class Paddle
    {
        private Field _field;
        public Vector2 position;
        public int width;
        public int height;
        public int speed;
        public Paddle(Vector2 position, int width, int height, int speed, Field field)
        {
            this.position = position;
            this.width = width;
            this.height = height;
            this.speed = speed;
            this._field = field;
        }

        public void MoveUp()
        {
            position.Y -= speed;
            if (position.Y < 0)
            {
                position.Y = 0;
            }
        }   

        public void MoveDown()
        {
            position.Y += speed;
            if (position.Y + height > _field.height)
            {
                position.Y = _field.height - height;
            }
        }

        public void Update(PaddleCommand command)
        {
            switch (command)
            {
                case PaddleCommand.Up:
                    MoveUp();
                    break;

                case PaddleCommand.Down:
                    MoveDown();
                    break;

                case PaddleCommand.None:
                    break;
            }
        }
    }
}
