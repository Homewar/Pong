namespace Pong
{
    internal class RemotePlayer : IController
    {
        private NetworkClient _network;

        public RemotePlayer(NetworkClient network)
        {
            _network = network;
        }

        public PaddleCommand GetCommand(Paddle paddle)
        {
            // RemotePlayer не двигает ракетку локально — сервер управляет всем
            return PaddleCommand.None;
        }
    }
}