using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
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
        
        private DeviceEventCallBack _deviceEventCallback;
        private GameObjectEventTrigger _gameObjectEventTrigger;
        
        public bool isEnabled { get; private set; }

        /// <summary>
        /// Set DeviceWatcher Enable State
        /// </summary>
        /// <exception cref="iMobileDevice.iDevice.iDeviceException"></exception>
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
        
        private void Start()
        {
            if (isEnabled)
                return;
            
            var go = new GameObject(nameof(GameObjectEventTrigger));
            go.hideFlags = HideFlags.HideAndDontSave;
            _gameObjectEventTrigger = go.AddComponent<GameObjectEventTrigger>();
            _gameObjectEventTrigger.updateCallback = Update;
            
            //_deviceEventCallback = GetDeviceEventCallback();

//#if UNITY_IOS

            LibiMobileDevice.Instance.iDevice.idevice_event_subscribe((ref iDeviceEvent deviceEvent, IntPtr data) =>
            {
                PendingEvents.Enqueue(new DeviceEvent(deviceEvent));
            }, IntPtr.Zero);
            try
            {
                
            }
            catch (Exception)
            {
                UnityEngine.Object.Destroy(_gameObjectEventTrigger.gameObject);
                _gameObjectEventTrigger = null;
                throw;
            }
//#endif

            
            isEnabled = true;
        }
        
        private void Stop()
        {
            if (!isEnabled)
                return;
            
            if (_gameObjectEventTrigger != null)
            {
                UnityEngine.Object.Destroy(_gameObjectEventTrigger.gameObject);
                _gameObjectEventTrigger = null;
            }

            _deviceEventCallback = null;
            isEnabled = false;
            LibiMobileDevice.Instance.iDevice.idevice_event_unsubscribe().ThrowOnError();
        }

        private void Update()
        {
            while (PendingEvents.TryDequeue(out var deviceEvent))
            {
                var deviceInfo = availableDevices.FirstOrDefault(d => d.udid == deviceEvent.udid);
                if (deviceInfo.udid != deviceEvent.udid)
                {
                    deviceInfo = new DeviceInfo(deviceEvent.udid, "", deviceEvent.connectionType);
                    PopulateDeviceName(ref deviceInfo);
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
        
        private static bool PopulateDeviceName(ref DeviceInfo deviceInfo)
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
                
                deviceInfo = new DeviceInfo(deviceInfo.udid, deviceName, deviceInfo.connectionType);
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
        
        private struct DeviceEvent
        {
            public string udid;
            public DeviceEventType eventType;
            public DeviceConnectionType connectionType;

            public DeviceEvent(iDeviceEvent deviceEvent)
            {
                udid = deviceEvent.udidString;
                eventType = (DeviceEventType)deviceEvent.@event;
                connectionType = (DeviceConnectionType)deviceEvent.conn_type;
            }
        }
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

    public class DeviceEventCallBack
    {
        public string udid { get; private set; }
        public DeviceEventType deviceEventType { get; private set; }
        public DeviceConnectionType deviceConnectionType { get; private set; }

        public DeviceEventCallBack(string udid, DeviceEventType deviceEventType, DeviceConnectionType deviceConnectionType)
        {
            this.udid = udid;
            this.deviceEventType = deviceEventType;
            this.deviceConnectionType = deviceConnectionType;
        }
    }
}