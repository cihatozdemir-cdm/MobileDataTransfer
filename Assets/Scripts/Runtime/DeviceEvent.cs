using AndroidLib.Unity;
using iMobileDevice.iDevice;

namespace MobileDataTransfer.Unity
{
    public struct DeviceEvent
    {
        public string udid;
        public DeviceType deviceType;
        public DeviceEventType eventType;
        public DeviceConnectionType connectionType;

        public DeviceEvent(string udid, DeviceType deviceType, DeviceEventType eventType, DeviceConnectionType connectionType)
        {
            this.udid = udid;
            this.deviceType = deviceType;
            this.eventType = eventType;
            this.connectionType = connectionType;
        }

        public DeviceEvent(iDeviceEvent deviceEvent)
        {
            udid = deviceEvent.udidString;
            eventType = (DeviceEventType)deviceEvent.@event;
            connectionType = (DeviceConnectionType)deviceEvent.conn_type;
            deviceType = DeviceType.IOS;
        }
        
        public DeviceEvent(aDeviceEvent deviceEvent)
        {
            udid = deviceEvent.serialNumber;
            eventType = (DeviceEventType)deviceEvent.eventType;
            connectionType = (DeviceConnectionType)deviceEvent.connectionType;
            deviceType = DeviceType.Android;
        }
    }
}