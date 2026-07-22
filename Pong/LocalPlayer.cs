namespace Pong
{
    internal class LocalPlayer : IController
    {
        private Input _input;
        private NetworkClient _network;

        public LocalPlayer(Input input, NetworkClient network)
        {
            _input = input;
            _network = network;
        }

        public PaddleCommand GetCommand(Paddle paddle)
        {
            PaddleCommand cmd;

            if (_input.W)
                cmd = PaddleCommand.Up;
            else if (_input.S)
                cmd = PaddleCommand.Down;
            else
                cmd = PaddleCommand.None;

            // Отправляем команду на сервер
            _network.SendCommand(cmd);
            return cmd;
        }
    }
}