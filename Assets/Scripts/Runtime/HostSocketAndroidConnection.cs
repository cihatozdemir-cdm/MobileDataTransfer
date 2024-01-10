using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MobileDataTransfer.Unity
{
    public class HostSocketAndroidConnection : ISocketConnection
    {
        public DeviceInfo deviceInfo { get; }

        private byte[] _buffer = new byte[4096];
        private Socket _socket;
        private Socket _deviceSocket;


        public HostSocketAndroidConnection(DeviceInfo deviceInfo)
        {
            this.deviceInfo = deviceInfo;
        }

        public void Dispose()
        {
            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Close();
            _socket?.Dispose();
            Debug.LogWarning("Socket Disposed");
            _deviceSocket?.Shutdown(SocketShutdown.Both);
            _deviceSocket?.Close();
            _deviceSocket?.Dispose();
        }

        public void Connect(int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
            
            //socket.Listen(1);
            //var deviceSocket = socket.Accept();
            //socket.
            socket.Connect(new IPEndPoint(IPAddress.Loopback, port));

            _socket = socket;
            //_deviceSocket = deviceSocket;
        }

        public void Disconnect()
        {
            _socket?.Disconnect(false);
            _deviceSocket?.Disconnect(false);
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