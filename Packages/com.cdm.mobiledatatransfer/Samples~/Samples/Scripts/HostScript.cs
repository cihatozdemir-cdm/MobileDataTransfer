#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using Cdm.MobileDataTransfer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cdm.MobileDeviceTransfer.Samples
{
    public class HostScript : MonoBehaviour
    {
        public float connectionWaitTime = 1f;
        public Texture2D textureToSend;
        public RawImage image;
        public TMP_Text deviceInfoText;
        public GameObject deviceWatcherControlPanel;
    
        private DeviceWatcher _deviceWatcher;
        private string _deviceId;

        private ISocketConnection socket;
        private CancellationTokenSource _cancellationTokenSource;
    
        private void OnEnable()
        {
            if (Application.platform != RuntimePlatform.WindowsEditor &&
                Application.platform != RuntimePlatform.WindowsPlayer &&
                Application.platform != RuntimePlatform.LinuxEditor &&
                Application.platform != RuntimePlatform.LinuxPlayer &&
                Application.platform != RuntimePlatform.OSXEditor &&
                Application.platform != RuntimePlatform.OSXPlayer)
            {
                enabled = false;
            }
            
            deviceWatcherControlPanel.SetActive(true);
        }

        private void OnDisable()
        {
            _cancellationTokenSource?.Cancel();
            socket?.Dispose();
        }

        private void OnDestroy()
        {
            if (_deviceWatcher != null)
            {
                _deviceWatcher.deviceAdded -= DeviceWatcher_OnDeviceAdded;
                _deviceWatcher.deviceRemoved -= DeviceWatcher_OnDeviceRemoved;
                _deviceWatcher.devicePaired -= DeviceWatcher_OnDevicePaired;
                _deviceWatcher.SetEnabled(false);
            }
        }

        private async void DeviceWatcher_OnDeviceAdded(DeviceEventArgs e)
        {
            Debug.Log($"Device added: {deviceInfoText.name} [{e.deviceInfo.udid}] [{e.deviceInfo.connectionType}]");

            if (!string.IsNullOrEmpty(_deviceId))
                return;

            if (e.deviceInfo.connectionType != DeviceConnectionType.Usbmuxd)
                return;
        
            deviceInfoText.SetText($"{deviceInfoText.name} [{e.deviceInfo.udid}] [{e.deviceInfo.connectionType}]");

            Debug.Log($"Trying to connect to the device on port {SocketTextureUtility.Port}...");

            _cancellationTokenSource = new CancellationTokenSource();
            var isConnected = false;
            while (!isConnected && !_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    socket = HostSocketConnection.CreateForTargetDevice(e.deviceInfo);
                    await socket.ConnectAsync(SocketTextureUtility.Port);
                    isConnected = true;
                }
                catch (Exception ex)
                {
                    Debug.Log($"Connection failed due to {ex}. Trying to connect after {connectionWaitTime} secs...");
                
                    isConnected = false;
                    socket?.Dispose();
                    socket = null;
                }

                try
                {
                    if (!isConnected)
                        await Task.Delay((int) (connectionWaitTime * 1000), _cancellationTokenSource.Token);
                }
                catch (Exception)
                {
                    return;
                }
            }

            if (socket == null)
                return;
        
            Debug.Log($"Connection has been established!");
            deviceInfoText.SetText($"Connection has been established! {e.deviceInfo.udid}");
            _deviceId = e.deviceInfo.udid;

            var success = await SocketTextureUtility.SendAsync(socket, textureToSend);
            if (success)
            {
                Debug.Log("Texture has been sent!");
            }
            else
            {
                Debug.LogWarning("Texture could not be sent!");
            }
        
            Debug.Log($"Waiting for ACK...");
            var ack = await socket.ReceiveInt32Async();
            Debug.Log($"Received  ACK: {(ack.HasValue ? "YES" : "NO")}");
        
            var texture = await SocketTextureUtility.ReceiveAsync(socket);
            if (texture != null)
            {
                Debug.Log("Texture has been received!");
            
                image.gameObject.SetActive(true);
                image.texture = texture;
            }
            else
            {
                Debug.Log("Texture could not be received.");   
            }
        
            Debug.Log($"Sending ACK...");
            await socket.SendInt32Async(1);
            Debug.Log($"DONE!");
        
            socket.Disconnect();
            socket.Dispose();

            _deviceId = string.Empty;
        }

        private void DeviceWatcher_OnDeviceRemoved(DeviceEventArgs e)
        {
            Debug.Log($"Device removed: {deviceInfoText.name} [{e.deviceInfo.udid}] [{e.deviceInfo.connectionType}]");
            _cancellationTokenSource?.Cancel(false);
        
            deviceInfoText.text = "Waiting for connection...";
        }

        private void DeviceWatcher_OnDevicePaired(DeviceEventArgs e)
        {
            Debug.Log($"Device paired: {deviceInfoText.name} [{e.deviceInfo.udid}] [{e.deviceInfo.connectionType}]");
            deviceInfoText.text = $"{deviceInfoText.name} [{e.deviceInfo.udid}] [{e.deviceInfo.connectionType}] [Paired]";
        }

        public void SetDeviceWatcherActive(bool isActive)
        {
            if (isActive)
            {
                _deviceWatcher = new DeviceWatcher();
                
                _deviceWatcher.deviceAdded += DeviceWatcher_OnDeviceAdded;
                _deviceWatcher.deviceRemoved += DeviceWatcher_OnDeviceRemoved;
                _deviceWatcher.devicePaired += DeviceWatcher_OnDevicePaired;
                _deviceWatcher.SetEnabled(true);
                Debug.Log("Device watcher running...");
            }
            else
            {
                if (_deviceWatcher != null)
                {
                    _deviceWatcher.deviceAdded -= DeviceWatcher_OnDeviceAdded;
                    _deviceWatcher.deviceRemoved -= DeviceWatcher_OnDeviceRemoved;
                    _deviceWatcher.devicePaired -= DeviceWatcher_OnDevicePaired;
                    _deviceWatcher.SetEnabled(false);
                    
                    _cancellationTokenSource?.Cancel(false);
                }
            }
        }
    }
}
#endif