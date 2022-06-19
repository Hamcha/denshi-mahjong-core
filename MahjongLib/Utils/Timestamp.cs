using System;

namespace MahjongLib.Utils
{
    public interface ITimestamp
    {
        public ulong Now { get; }
    }

    public class DefaultTime : ITimestamp
    {
        public ulong Now => (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}