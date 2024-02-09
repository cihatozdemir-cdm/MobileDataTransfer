using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AndroidLib.Unity.Extensions;
using RegawMOD.Android;

namespace AndroidLib.Unity
{
        public class AndroidConnectionManager
        {
                private AndroidController _androidController;
                private CancellationTokenSource _cancellationToken;
                
                private aDeviceEventCallback _onEventAnyDevice;
                private Task _deviceSearchTask;

                private readonly List<string> _connectedDevices = new List<string>();

                public delegate void aDeviceEventCallback(aDeviceEvent deviceEvent);

                /// <summary>
                /// Start Device Search
                /// </summary>
                public void StartSearch()
                {
                        if (_deviceSearchTask != null && _deviceSearchTask.Status == TaskStatus.Running) return;

                        _cancellationToken = new CancellationTokenSource();
                        _deviceSearchTask = Task.Run(SearchAndroidDevices);
                }

                /// <summary>
                /// Clear all data
                /// </summary>
                public void Dispose()
                {
                        _cancellationToken.Cancel();
                        _connectedDevices.Clear();
                        _androidController.Dispose();
                        
                        _onEventAnyDevice = null;
                        _deviceSearchTask = null;
                }
                
                /// <summary>
                /// Get Android Device Name
                /// </summary>
                /// <param name="serialNumber">Serial number of the device you want to name</param>
                /// <param name="deviceName"></param>
                /// <returns></returns>
                public aDeviceError GetDeviceName(string serialNumber, out string deviceName)
                {
                        var device = _androidController.GetConnectedDevice(serialNumber);

                        if (device == null)
                        {
                                deviceName = "UNKNOWN";
                                return aDeviceError.NoDevice;
                        }

                        deviceName = device.GetDeviceName();
                        return aDeviceError.Success;
                }

                
                /// <summary>
                /// Check any changes on Devices (Add New Device, Remove Device etc.)
                /// </summary>
                private async Task SearchAndroidDevices()
                {
                        const float searchRefreshDelayInSecond = 0.4f;

                        _androidController = AndroidConnLib.Instance.androidController;

                        while (!_cancellationToken.IsCancellationRequested)
                        {
                                var instantConnectedDevices = _androidController.ConnectedDevices;
                                //If the controlled device is not in the ConnectedDevices list, it means it has been newly added.
                                foreach (var connectedDevice in instantConnectedDevices)
                                {
                                        if (!_connectedDevices.Contains(connectedDevice))
                                        {
                                                _connectedDevices.Add(connectedDevice);
                                                _onEventAnyDevice?.Invoke(new aDeviceEvent(connectedDevice,
                                                        aDeviceEventType.DeviceAdd,
                                                        aDeviceConnectionType.Usbmuxd));
                                        }
                                }

                                //If the controlled device is not in the instantConnectedDevices list, it means it has been newly removed.
                                for (var i = 0; i < _connectedDevices.Count; i++)
                                {
                                        var connectedDevice = _connectedDevices[i];
                                        if (!instantConnectedDevices.Contains(connectedDevice))
                                        {
                                                _onEventAnyDevice?.Invoke(new aDeviceEvent(connectedDevice,
                                                        aDeviceEventType.DeviceRemove,
                                                        aDeviceConnectionType.Usbmuxd));
                                                _connectedDevices.Remove(connectedDevice);

                                                i--;
                                        }
                                }

                                await Task.Delay((int)(searchRefreshDelayInSecond * 1000), _cancellationToken.Token);
                        }
                }

                /// <summary>
                /// Register a callback function that will be called when device add/remove
                /// events occur.
                /// </summary>
                /// <param name="eventCallback">Callback function to call.</param>
                /// <returns>aDeviceError.Success on success or an error value when an error occurred.</returns>
                public aDeviceError SubscribeDeviceEvent(aDeviceEventCallback eventCallback)
                {
                        if (_onEventAnyDevice == null)
                        {
                                _onEventAnyDevice = eventCallback;
                                StartSearch();
                        }
                        else
                        {
                                _onEventAnyDevice += eventCallback;
                        }

                        return aDeviceError.Success;
                }

                /// <summary>
                /// Clear all data and Stop Searching devices 
                /// </summary>
                /// <returns></returns>
                public aDeviceError UnSubscribeAllDeviceEvents()
                {
                        Dispose();

                        return aDeviceError.Success;
                }

                /// <summary>
                /// Get All Connected Devices
                /// </summary>
                /// <param name="devices"></param>
                /// <returns></returns>
                public aDeviceError GetDeviceList(out IEnumerable<string> devices)
                {
                        devices = _connectedDevices;
                        return aDeviceError.Success;
                }
        }
}