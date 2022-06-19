using MahjongLib.Utils;

namespace DenshiMahjong.Utils
{
    public class GameLog : ILogger
    {
        public void Log(string msg)
        {
            Godot.GD.Print(msg);
        }
    }
}