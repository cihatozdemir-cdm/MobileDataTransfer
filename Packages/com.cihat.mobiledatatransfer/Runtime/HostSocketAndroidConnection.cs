#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AndroidLib.Unity;
using RegawMOD.Android;

namespace MobileDataTransfer.Unity
{
    public class HostSocketAndroidConnection : HostSocketConnection
    {
        private const string LocalHost = "127.0.0.1";

        private Socket _socket;


        public HostSocketAndroidConnection(DeviceInfo deviceInfo) : base(deviceInfo)
        {
        }

        /// <summary>
        /// Connect with target device using port
        /// </summary>
        /// <param name="port"></param>
        public override void Connect(int port)
        {
            var device = AndroidConnLib.Instance.androidController.GetConnectedDevice(deviceInfo.udid);
            if (device == null)
            {
                throw new aDeviceException(aDeviceError.NoDevice);
            }
            
            Adb.PortForward(device, port, port);
            //AdbExtensions.PortReverse(device, port, port);
            
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(LocalHost, port);
        }

        /// <summary>
        /// Disconnect from the device and clean up resources.
        /// </summary>
        public override void Disconnect()
        {
            _socket?.Disconnect(false);
            _socket = null;
        }
        
        /// <summary>
        /// Dispose Connection
        /// </summary>
        public override void Dispose()
        {
            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Close();
            _socket?.Dispose();
        }

        /// <summary>
        /// Send the buffer given to the device via the given connection.
        /// </summary>
        /// <param name="buffer">Buffer with data to send.</param>
        /// <param name="size">Size of the buffer to send.</param>
        /// <returns>The number of bytes actually sent.</returns>
        public override int Send(byte[] buffer, int size)
        {
            return SendInternal(buffer, size);
        }

        /// <summary>
        /// Receive data from a device via the given connection.
        /// </summary>
        /// <param name="buffer">Buffer that will be filled with the received data.</param>
        /// <param name="size">Size of the buffer to receive.</param>
        /// <returns>The number of bytes actually receive.</returns>
        public override int Receive(byte[] buffer, int size)
        {
            return ReceiveInternal(buffer, size);
        }

        /// <summary>
        /// Send data to the device via the given connection.
        /// </summary>
        /// <param name="buffer">>Buffer with data to send.</param>
        /// <param name="length">Size of the buffer to send.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="TaskCanceledException"></exception>
        /// <returns></returns>
        private int SendInternal(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            if (_socket == null)
                throw new InvalidOperationException("Socket is not connected.");
            
            var totalSentBytes = 0;

            while (totalSentBytes < length)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

                //Try to send data
                int sentBytes;
                
                try
                {
                    sentBytes = _socket.Send(buffer, totalSentBytes, length - totalSentBytes, SocketFlags.None);
                }
                catch (Exception)
                {
                    break;
                }
                
                if (sentBytes == 0)
                    break;

                totalSentBytes += sentBytes;
            }

            return totalSentBytes;
        }

        /// <summary>
        /// Receive data from a device via the given connection.
        /// </summary>
        /// <param name="buffer">Buffer that will be filled with the received data. This buffer has to be
        /// large enough to hold <see cref="length"/> bytes.</param>
        /// <param name="length">Buffer size or number of bytes to receive.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="TaskCanceledException"></exception>
        /// <returns></returns>
        private int ReceiveInternal(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            if (_socket == null)
                throw new InvalidOperationException("Socket is not connected.");
            
            var totalReceivedBytes = 0;

            while (totalReceivedBytes < length)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

                //Try to receive data
                int receivedBytes;
                
                try
                {
                    receivedBytes = 
                        _socket.Receive(buffer, totalReceivedBytes, length - totalReceivedBytes, SocketFlags.None);
                }
                catch (Exception)
                {
                    break;
                }
                
                if (receivedBytes == 0)
                    break;

                totalReceivedBytes += receivedBytes;
            }

            return totalReceivedBytes;
        }
    }
}
#endif