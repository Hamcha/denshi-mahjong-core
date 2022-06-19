using MahjongLib.Utils;

namespace DenshiMahjong.Utils
{
    public class GodotTime: ITimestamp
    {
        public ulong Now => Godot.Time.GetTicksUsec();
    }
}