namespace AndroidLib.Unity
{
    public static class aDeviceErrorExtensions
    {
        public static void ThrowOnError(this aDeviceError deviceError)
        {
            if (deviceError != aDeviceError.Success)
                throw new aDeviceException(deviceError);
        }
        
        public static void ThrowOnError(this aDeviceError deviceError, string message)
        {
            if (deviceError != aDeviceError.Success)
                throw new aDeviceException(deviceError, message);
        }
        
        public static bool IsError(this aDeviceError value) => value != aDeviceError.Success;
    }
}