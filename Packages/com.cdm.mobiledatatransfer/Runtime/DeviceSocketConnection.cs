using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cdm.MobileDataTransfer
{
    public class DeviceSocketConnection : ISocketConnection
    {
        private Socket _serverSocket;
        public Socket socket { get; private set; }

        public static DeviceSocketConnection CreateForTargetDevice(RuntimePlatform runtimePlatform)
        {
            return runtimePlatform switch
            {
                RuntimePlatform.Android => new DeviceSocketAndroidConnection(),
                RuntimePlatform.IPhonePlayer => new DeviceSocketConnection(),
                _ => null
            };
        }
        
        /// <summary>
        /// Connect with target device using socket
        /// </summary>
        /// <param name="acceptSocket"></param>
        public virtual void Connect(Socket acceptSocket)
        {
            if (socket != null && socket.Connected)
                throw new InvalidOperationException("Disconnect current socket before using a new socket.");

            socket?.Dispose();
            socket = acceptSocket;
        }

        /// <summary>
        /// Connect with target device using port
        /// </summary>
        /// <param name="port"></param>
        public virtual void Connect(int port)
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, port));
            _serverSocket.Listen(1);
            
            socket = _serverSocket.Accept();
        }

        /// <summary>
        /// Disconnect from the device and clean up resources.
        /// </summary>
        public void Disconnect()
        {
            if (socket != null && socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
            }

            if (_serverSocket != null && _serverSocket.Connected)
            {
                _serverSocket.Shutdown(SocketShutdown.Both);
                _serverSocket.Disconnect(false);
            }
        }
        
        /// <summary>
        /// Dispose Connection
        /// </summary>
        public void Dispose()
        {
            socket?.Close();
            socket = null;
            
            _serverSocket?.Close();
            _serverSocket = null;
        }

        /// <summary>
        /// Send the buffer given to the device via the given connection.
        /// </summary>
        /// <param name="buffer">Buffer with data to send.</param>
        /// <param name="length">Size of the buffer to send.</param>
        /// <returns>The number of bytes actually sent.</returns>
        public int Send(byte[] buffer, int length)
        {
            return SendInternal(buffer, length);
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
            return ReceiveInternal(buffer, length);
        }
        
        /// <summary>
        /// Async version <see cref="Send"/>.
        /// </summary>
        public async Task<int> SendAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => SendInternal(buffer, length, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Async version <see cref="Receive"/>.
        /// </summary>
        public async Task<int> ReceiveAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => ReceiveInternal(buffer, length, cancellationToken), cancellationToken);
        }
        
        private int SendInternal(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            if (socket == null)
                throw new InvalidOperationException("Socket is not connected.");
            
            var totalSentBytes = 0;

            while (totalSentBytes < length)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

                var sentBytes = socket.Send(buffer, totalSentBytes, length - totalSentBytes, SocketFlags.None);
                if (sentBytes == 0)
                    break;

                totalSentBytes += sentBytes;
            }

            return totalSentBytes;
        }

        private int ReceiveInternal(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            if (socket == null)
                throw new InvalidOperationException("Socket is not connected.");
            
            var totalReceivedBytes = 0;

            while (totalReceivedBytes < length)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

                var receivedBytes = 
                    socket.Receive(buffer, totalReceivedBytes, length - totalReceivedBytes, SocketFlags.None);
                if (receivedBytes == 0)
                    break;

                totalReceivedBytes += receivedBytes;
            }

            return totalReceivedBytes;
        }
    }
}