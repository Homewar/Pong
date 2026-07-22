using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pong
{
    public partial class Form1 : Form
    {
        public Input Input { get; } = new Input();
        private Game game;
        private Timer timer;
        public Form1()
        {
            InitializeComponent();

            Input = new Input();

            this.Load += Form1_Load;

            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            KeyPreview = true;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            game = new Game(Input);

            timer = new Timer();
            timer.Interval = 16;
            timer.Tick += GameLoop;
            timer.Start();

            // Подключение к серверу в фоне, чтобы не блокировать запуск игры
            await game.Ready();
        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Input.KeyDown(e.KeyCode);
        }

        void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            Input.KeyUp(e.KeyCode);
        }

        private void GameLoop(object sender, EventArgs e)
        {
            game.Run();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (game?.Bitmap != null)
            {
                e.Graphics.DrawImage(game.Bitmap, 0, 0);
            }
        }
    }
}
