using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AndroidLib.Unity;
using RegawMOD.Android;

namespace MobileDataTransfer.Unity
{
    public class HostSocketAndroidConnection : ISocketConnection
    {
        private const string LocalHost = "127.0.0.1";
        private DeviceInfo deviceInfo { get; }

        private Socket _socket;


        public HostSocketAndroidConnection(DeviceInfo deviceInfo)
        {
            this.deviceInfo = deviceInfo;
        }

        public void Connect(int port)
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

        public void Disconnect()
        {
            _socket?.Disconnect(false);
            _socket = null;
        }
        
        public void Dispose()
        {
            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Close();
            _socket?.Dispose();
        }

        public int Send(byte[] buffer, int size)
        {
            return SendInternal(buffer, size);
        }

        public int Receive(byte[] buffer, int size)
        {
            return ReceiveInternal(buffer, size);
        }

        /// <summary>
        /// Async version <see cref="Send"/>.
        /// </summary>
        public async Task<int> SendAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Send(buffer, length), cancellationToken);
        }

        /// <summary>
        /// Async version <see cref="Receive"/>.
        /// </summary>
        public async Task<int> ReceiveAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Receive(buffer, length), cancellationToken);
        }
        
        private int SendInternal(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            if (_socket == null)
                throw new InvalidOperationException("Socket is not connected.");
            
            var totalSentBytes = 0;

            while (totalSentBytes < length)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

                var sentBytes = _socket.Send(buffer, totalSentBytes, length - totalSentBytes, SocketFlags.None);
                if (sentBytes == 0)
                    break;

                totalSentBytes += sentBytes;
            }

            return totalSentBytes;
        }

        private int ReceiveInternal(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            if (_socket == null)
                throw new InvalidOperationException("Socket is not connected.");
            
            var totalReceivedBytes = 0;

            while (totalReceivedBytes < length)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

                var receivedBytes = 
                    _socket.Receive(buffer, totalReceivedBytes, length - totalReceivedBytes, SocketFlags.None);
                if (receivedBytes == 0)
                    break;

                totalReceivedBytes += receivedBytes;
            }

            return totalReceivedBytes;
        }
    }
}