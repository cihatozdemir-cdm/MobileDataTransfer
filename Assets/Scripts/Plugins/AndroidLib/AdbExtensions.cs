using System.IO;
using RegawMOD.Android;

namespace AndroidLib.Unity.Extensions
{
    public class AdbExtensions
    {
        public static bool PortReverse(Device device, int localPort, int remotePort)
        {
            bool flag = false;
            using (StringReader stringReader = new StringReader(Adb.ExecuteAdbCommand(Adb.FormAdbCommand(device, "reverse", (object) ("tcp:" + localPort.ToString()), (object) ("tcp:" + remotePort.ToString())))))
            {
                if (stringReader.ReadToEnd().Trim() == "")
                    flag = true;
            }
            return flag;
        }
    }
}