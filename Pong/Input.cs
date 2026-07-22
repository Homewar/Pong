using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pong
{
    public class Input
    {
        public bool W { get; private set; }
        public bool S { get; private set; }

        public void KeyDown(Keys key)
        {
            if (key == Keys.W)
                W = true;

            if (key == Keys.S)
                S = true;

            if (key == Keys.Escape)
                Application.Exit();
        }

        public void KeyUp(Keys key)
        {
            if (key == Keys.W)
                W = false;

            if (key == Keys.S)
                S = false;

            if (key == Keys.Escape)
                Application.Exit();
        }
    }
}
