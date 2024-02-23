using System;
using System.Net.Sockets;

namespace Cdm.MobileDataTransfer
{
    public class DeviceSocketAndroidConnection : DeviceSocketConnection
    {
        private const int TimeoutInMs = 15000;
        public override void Connect(Socket acceptSocket)
        {
            base.Connect(acceptSocket);
            
            Handshake();
        }

        public override void Connect(int port)
        {
            base.Connect(port);
            
            Handshake();
        }

        private bool Handshake()
        {
            try
            {
                var getRequest = this.ReceiveInt32Async();
                if (!getRequest.Wait(TimeoutInMs)) throw new Exception("Timeout!");
                var sendRequest = this.SendInt32Async(1);
                if (sendRequest.Wait(TimeoutInMs) && sendRequest.Result)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }
}