namespace MobileDataTransfer.Unity
{
    /// <summary>Error Codes</summary>
    public enum DeviceError
    {
        Timeout = -7, // 0xFFFFFFF9
        SslError = -6, // 0xFFFFFFFA
        NotEnoughData = -4, // 0xFFFFFFFC
        NoDevice = -3, // 0xFFFFFFFD
        UnknownError = -2, // 0xFFFFFFFE
        InvalidArg = -1, // 0xFFFFFFFF
        Success = 0,
    }
    
    
}