#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using iMobileDevice;
using iMobileDevice.iDevice;

namespace MobileDataTransfer.Unity
{
    public class HostSocketIOSConnection : HostSocketConnection
    {
        private readonly byte[] _buffer = new byte[4096];
        private iDeviceHandle _deviceHandle;
        private iDeviceConnectionHandle _connectionHandle;
        
        public HostSocketIOSConnection(DeviceInfo deviceInfo) : base(deviceInfo)
        {
        }
        
        /// <summary>
        /// Connect with target device using port
        /// </summary>
        /// <param name="port"></param>
        public override void Connect(int port)
        {
            iDeviceHandle deviceHandle = null;
            iDeviceConnectionHandle connectionHandle = null;

            try
            {
                var deviceApi = LibiMobileDevice.Instance.iDevice;
                deviceApi.idevice_new(out deviceHandle, deviceInfo.udid).ThrowOnError();
                deviceApi.idevice_connect(deviceHandle, (ushort) port, out connectionHandle).ThrowOnError();
                _deviceHandle = deviceHandle;
                _connectionHandle = connectionHandle;
            }
            catch (Exception)
            {
                deviceHandle?.Dispose();
                connectionHandle?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Disconnect from the device and clean up resources.
        /// </summary>
        public override void Disconnect()
        {
            _deviceHandle?.Dispose();
            _deviceHandle = null;
            
            _connectionHandle?.Dispose();
            _connectionHandle = null;
        }
        
        /// <summary>
        /// Dispose Connection
        /// </summary>
        public override void Dispose()
        {
            _deviceHandle?.Dispose();
            _connectionHandle?.Dispose();
        }

        /// <summary>
        /// Send the buffer given to the device via the given connection.
        /// </summary>
        /// <param name="buffer">Buffer with data to send.</param>
        /// <param name="size">Size of the buffer to send.</param>
        /// <returns>The number of bytes actually sent.</returns>
        public override int Send(byte[] buffer, int size)
        {
            var deviceApi = LibiMobileDevice.Instance.iDevice;

            uint sentBytes = 0;
            deviceApi.idevice_connection_send(_connectionHandle, buffer, (uint) size, ref sentBytes).ThrowOnError();
            return (int) sentBytes;
        }

        /// <summary>
        /// Receive data from a device via the given connection.
        /// </summary>
        /// <param name="buffer">Buffer that will be filled with the received data. This buffer has to be
        /// large enough to hold <see cref="length"/> bytes.</param>
        /// <param name="length">Buffer size or number of bytes to receive.</param>
        /// <returns></returns>
        public override int Receive(byte[] buffer, int length)
        {
            var deviceApi = LibiMobileDevice.Instance.iDevice;

            // Use actual buffer if given buffer length is small.
            if (_buffer.Length > length)
            {
                uint recvBytes = 0;

                if (receiveTimeout == 0)
                {
                    deviceApi.idevice_connection_receive(_connectionHandle, buffer, (uint) length, ref recvBytes)
                        .ThrowOnError();
                }
                else
                {
                    deviceApi.idevice_connection_receive_timeout(
                        _connectionHandle, buffer, (uint) length, ref recvBytes, receiveTimeout).ThrowOnError(); 
                }

                return (int) recvBytes;
            }

            // Read buffered.
            var recvTotal = 0;

            while (recvTotal < length)
            {
                var lengthRead = Math.Min(length - recvTotal, _buffer.Length);

                uint recv = 0;

                if (receiveTimeout == 0)
                {
                    deviceApi.idevice_connection_receive(_connectionHandle, _buffer, (uint)lengthRead, ref recv)
                        .ThrowOnError();
                }
                else
                {
                    deviceApi.idevice_connection_receive_timeout(
                        _connectionHandle, _buffer, (uint) lengthRead, ref recv, receiveTimeout).ThrowOnError(); 
                }

                if (recv == 0)
                {
                    break;
                }
                
                Array.Copy(_buffer, 0, buffer, recvTotal, recv);
                recvTotal += (int) recv;
            }

            return recvTotal;
        }
    }
}
#endif