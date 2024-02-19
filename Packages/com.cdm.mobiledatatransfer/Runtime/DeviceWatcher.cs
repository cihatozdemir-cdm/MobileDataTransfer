#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using iMobileDevice;
using iMobileDevice.iDevice;
using AndroidLib.Unity;
using Cdm.MobileDataTransfer.Extensions;
using RegawMOD.Android;
using UnityEngine;

namespace Cdm.MobileDataTransfer
{
    public class DeviceWatcher 
    {
        private const string Label = "MobileDataTransfer.Unity.DeviceWatcher";
        
        private readonly HashSet<DeviceInfo> _availableDevices = new HashSet<DeviceInfo>();
        private readonly ConcurrentQueue<DeviceEvent> _pendingEvents = new ConcurrentQueue<DeviceEvent>();

        private GameObjectEventTrigger _gameObjectEventTrigger;
        private GCHandle _instanceHandle;

        /// <summary>
        /// Gets the available devices.
        /// </summary>
        public IReadOnlyCollection<DeviceInfo> availableDevices => _availableDevices;
        
        /// <summary>
        /// Get Device Watcher Enable state
        /// </summary>
        public bool isEnabled { get; private set; }
        /// <summary>
        /// IOS Events is subscribed successfully
        /// </summary>
        public bool isIOSInitialized { get; private set; }
        /// <summary>
        /// Android Events is subscribed successfully
        /// </summary>
        public bool isAndroidInitialized { get; private set; }

        /// <summary>
        /// Set DeviceWatcher Enable State
        /// </summary>
        /// <exception cref="iMobileDevice.iDevice.iDeviceException"></exception>
        /// <exception cref="aDeviceException"></exception>
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
            
            _instanceHandle = GCHandle.Alloc(this);

            try
            {
                //IOS Subscribe Callback
                LibiMobileDevice.Instance.iDevice.idevice_event_subscribe((ref iDeviceEvent deviceEvent, IntPtr data) =>
                {
                    _pendingEvents.Enqueue(new DeviceEvent(deviceEvent));
                }, GCHandle.ToIntPtr(_instanceHandle)).ThrowOnError();

                isIOSInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

            try
            {
                //Android Subscribe Callback
                AndroidConnLib.Instance.androidConnectionManager.SubscribeDeviceEvent(deviceEvent =>
                {
                    _pendingEvents.Enqueue(new DeviceEvent(deviceEvent));
                }).ThrowOnError();

                isAndroidInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
            
            if (isAndroidInitialized || isIOSInitialized)
                isEnabled = true;
            else
            {
                isEnabled = false;
                UnityEngine.Object.Destroy(_gameObjectEventTrigger.gameObject);
                _gameObjectEventTrigger = null;
                
                throw new Exception("DeviceWatcher can't running.");
            }
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

            _instanceHandle.Free();
            isEnabled = false;

            if (isIOSInitialized)
            {
                LibiMobileDevice.Instance.iDevice.idevice_event_unsubscribe().ThrowOnError();
                isIOSInitialized = false;
            }

            if (isAndroidInitialized)
            {
                AndroidConnLib.Instance.androidConnectionManager.UnSubscribeAllDeviceEvents().ThrowOnError();
                isAndroidInitialized = false;
            }
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