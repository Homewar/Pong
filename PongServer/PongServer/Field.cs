using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Field
    {
        public int width;
        public int height;
        
        public Field(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public bool IsBallOutOfBounds(Ball ball)
        {
            return ball.Position.X < 0 || ball.Position.X > width || ball.Position.Y < 0 || ball.Position.Y > height;
        }

        public bool IsGoalLeft(Ball ball)
        {
            return ball.Position.X < 0;
        }

        public bool IsGoalRight(Ball ball)
        {
            return ball.Position.X > width;
        }
    }
}
