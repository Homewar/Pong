using System.IO;

namespace Pong
{
    public struct GameState
    {
        public float BallX { get; set; }
        public float BallY { get; set; }

        public float Player1Y { get; set; }
        public float Player2Y { get; set; }

        public byte Score1 { get; set; }
        public byte Score2 { get; set; }

        public static GameState FromBytes(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                return new GameState
                {
                    BallX = reader.ReadSingle(),
                    BallY = reader.ReadSingle(),
                    Player1Y = reader.ReadSingle(),
                    Player2Y = reader.ReadSingle(),
                    Score1 = reader.ReadByte(),
                    Score2 = reader.ReadByte()
                };
            }
        }
    }
}