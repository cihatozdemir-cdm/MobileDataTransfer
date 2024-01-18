#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDataTransfer.Unity
{
    public abstract class HostSocketConnection : ISocketConnection
    {
        public DeviceInfo deviceInfo { get; }
  
        /// <summary>
        /// Timeout in milliseconds after which this function should return even if no data has been received.
        /// </summary>
        public uint receiveTimeout { get; set; } = 0;

        public HostSocketConnection(DeviceInfo deviceInfo)
        {
            this.deviceInfo = deviceInfo;
        }

        /// <summary>
        /// Get Target Device Connection class To Connect with.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static HostSocketConnection CreateForTargetDevice(DeviceInfo deviceInfo)
        {
            return deviceInfo.deviceType switch
            {
                DeviceType.Android => new HostSocketAndroidConnection(deviceInfo),
                DeviceType.IOS => new HostSocketIOSConnection(deviceInfo),
                DeviceType.Unknown => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// Connect with target device using port
        /// </summary>
        /// <param name="port"></param>
        public abstract void Connect(int port);

        /// <summary>
        /// Disconnect from the device and clean up resources.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Dispose Connection
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Send the buffer given to the device via the given connection.
        /// </summary>
        /// <param name="buffer">Buffer with data to send.</param>
        /// <param name="size">Size of the buffer to send.</param>
        /// <returns>The number of bytes actually sent.</returns>
        public abstract int Send(byte[] buffer, int size);

        /// <summary>
        /// Receive data from a device via the given connection.
        /// </summary>
        /// <param name="buffer">Buffer that will be filled with the received data. This buffer has to be
        /// large enough to hold <see cref="length"/> bytes.</param>
        /// <param name="length">Buffer size or number of bytes to receive.</param>
        /// <returns></returns>
        public abstract int Receive(byte[] buffer, int length);
        
        /// <summary>
        /// Async version <see cref="Send"/>.
        /// </summary>
        public virtual async Task<int> SendAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Send(buffer, length), cancellationToken);
        }

        /// <summary>
        /// Async version <see cref="Receive"/>.
        /// </summary>
        public virtual async Task<int> ReceiveAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Receive(buffer, length), cancellationToken);
        }

    }
}
#endif