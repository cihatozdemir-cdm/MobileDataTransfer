#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using iMobileDevice;
using iMobileDevice.iDevice;
using AndroidLib.Unity;
using MobileDataTransfer.Unity.Extensions;
using UnityEngine;

namespace MobileDataTransfer.Unity
{
    public class DeviceWatcher 
    {
        private const string Label = "MobileDataTransfer.Unity.DeviceWatcher";
        
        private readonly HashSet<DeviceInfo> _availableDevices = new HashSet<DeviceInfo>();
        private readonly ConcurrentQueue<DeviceEvent> _pendingEvents = new ConcurrentQueue<DeviceEvent>();

        private GameObjectEventTrigger _gameObjectEventTrigger;

        /// <summary>
        /// Gets the available devices.
        /// </summary>
        public IReadOnlyCollection<DeviceInfo> availableDevices => _availableDevices;
        
        /// <summary>
        /// Get Device Watcher Enable state
        /// </summary>
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
                    _pendingEvents.Enqueue(new DeviceEvent(deviceEvent));
                }, IntPtr.Zero).ThrowOnError();
                
                //Android Subscribe Callback
                AndroidConnLib.Instance.androidConnectionManager.SubscribeDeviceEvent(deviceEvent =>
                {
                    _pendingEvents.Enqueue(new DeviceEvent(deviceEvent));
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
            while (!_pendingEvents.IsEmpty)
            {
                if (!_pendingEvents.TryDequeue(out var deviceEvent)) break;
                
                var deviceInfo = availableDevices.FirstOrDefault(d => d.udid == deviceEvent.udid);
                if (deviceInfo.udid != deviceEvent.udid)
                {
                    deviceInfo = new DeviceInfo(deviceEvent.udid, "", deviceEvent.deviceType, deviceEvent.connectionType);

                    //Get Device Name
                    deviceInfo.PopulateDeviceName(Label);
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