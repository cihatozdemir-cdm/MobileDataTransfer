using System;
using System.Net.Sockets;

namespace Cdm.MobileDataTransfer
{
    public class DeviceSocketAndroidConnection : DeviceSocketConnection
    {
        private const int TimeoutInMs = 1000;
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

        private void Handshake()
        {
            try
            {
                var getRequest = this.ReceiveInt32Async();
                if (getRequest.Wait(TimeoutInMs) && getRequest.Result == 1)
                {
                    var sendRequest = this.SendInt32Async(1);
                    if (sendRequest.Wait(TimeoutInMs) && sendRequest.Result)
                    {
                        return;
                    }
                }
                
                Dispose();
            }
            catch (Exception)
            {
                Dispose();
            }
        }
    }
}