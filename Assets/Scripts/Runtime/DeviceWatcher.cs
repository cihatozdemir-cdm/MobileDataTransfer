#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using AndroidLib.Unity;
using UnityEngine;

namespace MobileDataTransfer.Unity
{
    public class DeviceWatcher 
    {
        private const string Label = "MobileDataTransfer.Unity.DeviceWatcher";
        
        private readonly HashSet<DeviceInfo> _availableDevices = new HashSet<DeviceInfo>();

        /// <summary>
        /// Gets the available devices.
        /// </summary>
        public IReadOnlyCollection<DeviceInfo> availableDevices => _availableDevices;
        private readonly ConcurrentQueue<DeviceEvent> PendingEvents = new ConcurrentQueue<DeviceEvent>();

        private GameObjectEventTrigger _gameObjectEventTrigger;
        
        public bool isEnabled { get; private set; }

        /// <summary>
        /// Set DeviceWatcher Enable State
        /// </summary>
        /// <exception cref="iMobileDevice.iDevice.iDeviceException"></exception>
        /// <exception cref="AndroidLib.Unity.aDeviceException"></exception>
        public void SetEnabled(bool enable)
        {
            if (enable)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }
        
        /// <summary>
        /// Initialize the DeviceWatcher and Subscribe Callbacks when Device Connections changed (Android & IOS) 
        /// </summary>
        private void Start()
        {
            if (isEnabled)
                return;
            
            var go = new GameObject(nameof(GameObjectEventTrigger));
            go.hideFlags = HideFlags.HideAndDontSave;
            _gameObjectEventTrigger = go.AddComponent<GameObjectEventTrigger>();
            _gameObjectEventTrigger.updateCallback = Update;

            try
            {
                //IOS Subscribe Callback
                LibiMobileDevice.Instance.iDevice.idevice_event_subscribe((ref iDeviceEvent deviceEvent, IntPtr data) =>
                {
                    PendingEvents.Enqueue(new DeviceEvent(deviceEvent));
                }, IntPtr.Zero).ThrowOnError();
                
                //Android Subscribe Callback
                AndroidConnLib.Instance.androidConnectionManager.SubscribeDeviceEvent(deviceEvent =>
                {
                    PendingEvents.Enqueue(new DeviceEvent(deviceEvent));
                }).ThrowOnError();

            }
            catch (Exception)
            {
                UnityEngine.Object.Destroy(_gameObjectEventTrigger.gameObject);
                _gameObjectEventTrigger = null;
                throw;
            }
            
            isEnabled = true;
        }
        
        /// <summary>
        /// Clear all data and Unsubscribe the Callbacks
        /// </summary>
        private void Stop()
        {
            if (!isEnabled)
                return;
            
            if (_gameObjectEventTrigger != null)
            {
                UnityEngine.Object.Destroy(_gameObjectEventTrigger.gameObject);
                _gameObjectEventTrigger = null;
            }

            isEnabled = false;
            
            LibiMobileDevice.Instance.iDevice.idevice_event_unsubscribe().ThrowOnError();
            AndroidConnLib.Instance.androidConnectionManager.UnSubscribeAllDeviceEvents().ThrowOnError();
        }

        /// <summary>
        /// Check is Any Event Pending
        /// </summary>
        private void Update()
        {
            while (PendingEvents.TryDequeue(out var deviceEvent))
            {
                var deviceInfo = availableDevices.FirstOrDefault(d => d.udid == deviceEvent.udid);
                if (deviceInfo.udid != deviceEvent.udid)
                {
                    deviceInfo = new DeviceInfo(deviceEvent.udid, "", deviceEvent.deviceType, deviceEvent.connectionType);

                    //Get Device Name
                    switch (deviceInfo.deviceType)
                    {
                        case DeviceType.Android:
                            PopulateAndroidDeviceName(ref deviceInfo);
                            break;
                        case DeviceType.IOS:
                            PopulateIOSDeviceName(ref deviceInfo);
                            break;
                        case DeviceType.Unknown:
                        default:
                            break;
                    }
                }
                
                switch (deviceEvent.eventType)
                {
                    case DeviceEventType.DeviceAdd:
                        _availableDevices.Add(deviceInfo);
                        OnDeviceAdded(new DeviceEventArgs(deviceInfo));
                        break;
                    case DeviceEventType.DeviceRemove:
                        _availableDevices.Remove(deviceInfo);
                        OnDeviceRemoved(new DeviceEventArgs(deviceInfo));
                        break;
                    case DeviceEventType.DevicePaired:
                        OnDevicePaired(new DeviceEventArgs(deviceInfo));
                        break;
                    default:
                        return;
                }
            }
        }
        
        private static bool PopulateIOSDeviceName(ref DeviceInfo deviceInfo)
        {
            iDeviceHandle deviceHandle = null;
            LockdownClientHandle lockdownClientHandle = null;

            try
            {
                var deviceApi = LibiMobileDevice.Instance.iDevice;
                var lockdownApi = LibiMobileDevice.Instance.Lockdown;

                deviceApi.idevice_new(out deviceHandle, deviceInfo.udid)
                    .ThrowOnError();

                lockdownApi.lockdownd_client_new_with_handshake(deviceHandle, out lockdownClientHandle, Label)
                    .ThrowOnError();

                lockdownApi.lockdownd_get_device_name(lockdownClientHandle, out var deviceName)
                    .ThrowOnError();
                
                deviceInfo = new DeviceInfo(deviceInfo.udid, deviceName, deviceInfo.deviceType, deviceInfo.connectionType);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                return false;
            }
            finally
            {
                deviceHandle?.Dispose();
                lockdownClientHandle?.Dispose();
            }
        }

        private static bool PopulateAndroidDeviceName(ref DeviceInfo deviceInfo)
        {
            try
            {
                var androidConnectionManager = AndroidConnLib.Instance.androidConnectionManager;
                androidConnectionManager.GetDeviceName(deviceInfo.udid, out var deviceName)
                    .ThrowOnError();

                deviceInfo = new DeviceInfo(deviceInfo.udid, deviceName, deviceInfo.deviceType, deviceInfo.connectionType);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        protected virtual void OnDeviceAdded(DeviceEventArgs e)
        {
            deviceAdded?.Invoke(e);
        }

        protected virtual void OnDeviceRemoved(DeviceEventArgs e)
        {
            deviceRemoved?.Invoke(e);
        }

        protected virtual void OnDevicePaired(DeviceEventArgs e)
        {
            devicePaired?.Invoke(e);
        }

        public event Action<DeviceEventArgs> deviceAdded;
        public event Action<DeviceEventArgs> deviceRemoved;
        public event Action<DeviceEventArgs> devicePaired;
    }

    public readonly struct DeviceEventArgs
    {
        /// <summary>
        /// Device information.
        /// </summary>
        public DeviceInfo deviceInfo { get; }

        public DeviceEventArgs(DeviceInfo deviceInfo)
        {
            this.deviceInfo = deviceInfo;
        }
    }
}
#endif