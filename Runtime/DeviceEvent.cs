#if UNITY_ANDROID || UNITY_STANDALONE || UNITY_EDITOR
using AndroidLib.Unity;
#endif
#if UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR
using iMobileDevice.iDevice;
#endif

namespace Cdm.MobileDataTransfer
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

#if UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR
        public DeviceEvent(iDeviceEvent deviceEvent)
        {
            udid = deviceEvent.udidString;
            eventType = (DeviceEventType)deviceEvent.@event;
            connectionType = (DeviceConnectionType)deviceEvent.conn_type;
            deviceType = DeviceType.IOS;
        }
#endif
      
#if UNITY_ANDROID || UNITY_STANDALONE || UNITY_EDITOR
        public DeviceEvent(aDeviceEvent deviceEvent)
        {
            udid = deviceEvent.serialNumber;
            eventType = (DeviceEventType)deviceEvent.eventType;
            connectionType = (DeviceConnectionType)deviceEvent.connectionType;
            deviceType = DeviceType.Android;
        }
#endif
    }
}