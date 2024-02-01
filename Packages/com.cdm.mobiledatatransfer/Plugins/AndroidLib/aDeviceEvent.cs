namespace AndroidLib.Unity
{
    public struct aDeviceEvent
    {
        public string serialNumber;
        public aDeviceEventType eventType;
        public aDeviceConnectionType connectionType;

        public aDeviceEvent(string serialNumber, aDeviceEventType eventType, aDeviceConnectionType connectionType)
        {
            this.serialNumber = serialNumber;
            this.eventType = eventType;
            this.connectionType = connectionType;
        }
    }
}