using System.Text;
using RegawMOD.Android;

namespace AndroidLib.Unity.Extensions
{
    public static class AndroidDeviceExtensions
    {
            /// <summary>
            /// Print Device Name
            /// </summary>
            /// <param name="device">The device you want to print name</param>
            /// <param name="appendModel">Append model to end of name</param>
            /// <param name="appendSerialNumber">Append serial number to end of name</param>
            /// <returns></returns>
            public static string GetDeviceName(this Device device, bool appendModel = true, bool appendSerialNumber = false) {
                const string DEVICE_MODEL_PROPERTY = "ro.product.model"; //$NON-NLS-1$
                const string DEVICE_MANUFACTURER_PROPERTY = "ro.product.manufacturer"; //$NON-NLS-1$

                const char SEPARATOR = ' ';

                var manufacturer = CleanupStringForDisplay(
                        device.BuildProp.GetProp(DEVICE_MANUFACTURER_PROPERTY));
                var model = device.BuildProp.GetProp(DEVICE_MODEL_PROPERTY);

                var sb = new StringBuilder(20);

                if (manufacturer != null) {
                        sb.Append(manufacturer);
                        sb[0] = char.ToUpper(sb[0]);
                }

                if (model != null && appendModel) {
                        sb.Append(SEPARATOR);
                        sb.Append(model);
                }

                if (appendSerialNumber)
                        sb.Append(SEPARATOR)
                                .Append(device.SerialNumber);
                
                return sb.ToString();
        }

        private static string CleanupStringForDisplay(string s) {
                if (s == null) {
                        return null;
                }

                var sb = new StringBuilder(s.Length);
                foreach (var c in s)
                {
                        sb.Append(char.IsLetterOrDigit(c) ? char.ToLower(c) : '_');
                }

                return sb.ToString();
        }       
    }
}