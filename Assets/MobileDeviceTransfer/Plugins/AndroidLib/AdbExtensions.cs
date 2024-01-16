using System.IO;
using RegawMOD.Android;

namespace AndroidLib.Unity.Extensions
{
    public class AdbExtensions
    {
        public static bool PortReverse(Device device, int localPort, int remotePort)
        {
            var flag = false;
            using var stringReader = new StringReader(
                Adb.ExecuteAdbCommand(Adb.FormAdbCommand(device, "reverse", "tcp:" + localPort, "tcp:" + remotePort)));
            
            if (stringReader.ReadToEnd()?.Trim() == "")
                flag = true;
            
            return flag;
        }
    }
}