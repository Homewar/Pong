namespace Pong
{
    internal interface IController
    {
        PaddleCommand GetCommand(Paddle paddle);
    }
}
