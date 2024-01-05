using System;

namespace MobileDataTransfer.Unity
{
    public readonly struct DeviceInfo : IEquatable<DeviceInfo>
    {
        /// <summary>
        /// UDID of the device.
        /// </summary>
        public string udid { get; }

        /// <summary>
        /// Name of the device.
        /// </summary>
        public string name { get; }

        /// <summary>
        /// Connection type of the device.
        /// </summary>
        public DeviceConnectionType connectionType { get; }

        public DeviceInfo(string udid, string name, DeviceConnectionType connectionType)
        {
            this.udid = udid;
            this.name = name;
            this.connectionType = connectionType;
        }

        public bool Equals(DeviceInfo other)
        {
            return udid == other.udid;
        }

        public override bool Equals(object obj)
        {
            return obj is DeviceInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (udid != null ? udid.GetHashCode() : 0);
        }

        public static bool operator ==(DeviceInfo left, DeviceInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DeviceInfo left, DeviceInfo right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{udid} ({name})";
        }
    }
}