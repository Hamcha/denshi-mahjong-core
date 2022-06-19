using System.Diagnostics;

namespace MahjongLib.Utils
{
    public interface ILogger
    {
        public void Log(string message);
    }
    
    public class DefaultLogger : ILogger
    {
        public void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }
}