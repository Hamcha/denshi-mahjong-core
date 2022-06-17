namespace DenshiMahjong.Utils
{
    public class GameLog
    {
        public static void Log(string msg)
        {
            Godot.GD.Print(msg);
        }
    }
}