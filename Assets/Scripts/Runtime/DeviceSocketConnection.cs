using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDataTransfer.Unity
{
    public class DeviceSocketConnection : ISocketConnection
    {
        private Socket _serverSocket;
        private Socket _socket;

        public void Connect(int port)
        {
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, port));
            serverSocket.Listen(1);
            
            var socket = serverSocket.Accept();

            _serverSocket = serverSocket;
            _socket = socket;
        }

        public void Disconnect()
        {
            _serverSocket?.Disconnect(false);
            _socket?.Disconnect(false);
        }

        public int Send(byte[] buffer, int length)
        {
            return SendInternal(buffer, length);
        }

        public int Receive(byte[] buffer, int length)
        {
            return ReceiveInternal(buffer, length);
        }
        
        public async Task<int> SendAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => SendInternal(buffer, length, cancellationToken), cancellationToken);
        }

        public async Task<int> ReceiveAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => ReceiveInternal(buffer, length, cancellationToken), cancellationToken);
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
        
        public void Dispose()
        {
            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Close();
            _socket?.Dispose();
            
            _serverSocket?.Shutdown(SocketShutdown.Both);
            _serverSocket?.Close();
            _serverSocket?.Dispose();
        }
    }
}