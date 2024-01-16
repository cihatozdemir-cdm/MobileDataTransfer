#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDataTransfer.Unity
{
    public class HostSocketConnection : ISocketConnection
    {
        public DeviceInfo deviceInfo { get; }

        private readonly ISocketConnection _targetSocketConnection;
        
        public HostSocketConnection(DeviceInfo deviceInfo)
        {
            this.deviceInfo = deviceInfo;

            _targetSocketConnection = GetTargetDeviceSocketConnection();
        }

        /// <summary>
        /// Get Target Device Connection class To Connect with.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private ISocketConnection GetTargetDeviceSocketConnection()
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
        public void Connect(int port)
        {
            _targetSocketConnection.Connect(port);
        }

        /// <summary>
        /// Disconnect from the device and clean up resources.
        /// </summary>
        public void Disconnect()
        {
            _targetSocketConnection.Disconnect();
        }
        
        /// <summary>
        /// Dispose Connection
        /// </summary>
        public void Dispose()
        {
            _targetSocketConnection.Dispose();
        }

        /// <summary>
        /// Send the buffer given to the device via the given connection.
        /// </summary>
        /// <param name="buffer">Buffer with data to send.</param>
        /// <param name="size">Size of the buffer to send.</param>
        /// <returns>The number of bytes actually sent.</returns>
        public int Send(byte[] buffer, int size)
        {
            return _targetSocketConnection.Send(buffer, size);
        }

        /// <summary>
        /// Receive data from a device via the given connection.
        /// </summary>
        /// <param name="buffer">Buffer that will be filled with the received data. This buffer has to be
        /// large enough to hold <see cref="length"/> bytes.</param>
        /// <param name="length">Buffer size or number of bytes to receive.</param>
        /// <returns></returns>
        public int Receive(byte[] buffer, int length)
        {
            return _targetSocketConnection.Receive(buffer, length);
        }
        
        /// <summary>
        /// Async version <see cref="Send"/>.
        /// </summary>
        public async Task<int> SendAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            return await _targetSocketConnection.SendAsync(buffer, length, cancellationToken);
        }

        /// <summary>
        /// Async version <see cref="Receive"/>.
        /// </summary>
        public async Task<int> ReceiveAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            return await _targetSocketConnection.ReceiveAsync(buffer, length, cancellationToken);
        }

    }
}
#endif