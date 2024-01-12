using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AndroidLib.Unity.Extensions;
using UnityEngine;
using RegawMOD.Android;

namespace AndroidLib.Unity
{
        public class AndroidConnectionManager
        {
                private readonly AndroidController _androidController;
                private aDeviceEventCallback _onEventAnyDevice;
                private Task _deviceSearchTask;
                private readonly CancellationTokenSource _cancellationToken;

                public readonly List<string> ConnectedDevices = new List<string>();

                public delegate void aDeviceEventCallback(aDeviceEvent deviceEvent);

                public AndroidConnectionManager()
                {
                        _androidController = AndroidController.Instance;
                        _cancellationToken = new CancellationTokenSource();
                }

                public void StartSearch()
                {
                        if (_deviceSearchTask != null && _deviceSearchTask.Status == TaskStatus.Running) return;
                        
                        _deviceSearchTask = Task.Run(SearchAndroidDevices);
                }

                /// <summary>
                /// Clear all data
                /// </summary>
                public void Dispose()
                {
                        _cancellationToken.Cancel();
                        ConnectedDevices.Clear();
                        
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
                        const int searchRefreshDelayInSecond = 1;
                        
                        Debug.Log("Search Connected Android Devices");

                        while (!_cancellationToken.IsCancellationRequested)
                        {
                                var instantConnectedDevices = _androidController.ConnectedDevices;

                                //If the controlled device is not in the ConnectedDevices list, it means it has been newly added.
                                foreach (var connectedDevice in instantConnectedDevices)
                                {
                                        if (!ConnectedDevices.Contains(connectedDevice))
                                        {
                                                ConnectedDevices.Add(connectedDevice);
                                                _onEventAnyDevice?.Invoke(new aDeviceEvent(connectedDevice,
                                                        aDeviceEventType.DeviceAdd,
                                                        aDeviceConnectionType.Usbmuxd));
                                        }
                                }

                                //If the controlled device is not in the instantConnectedDevices list, it means it has been newly removed.
                                foreach (var connectedDevice in ConnectedDevices)
                                {
                                        if (!instantConnectedDevices.Contains(connectedDevice))
                                        {
                                                _onEventAnyDevice?.Invoke(new aDeviceEvent(connectedDevice,
                                                        aDeviceEventType.DeviceRemove,
                                                        aDeviceConnectionType.Usbmuxd));
                                                ConnectedDevices.Remove(connectedDevice);
                                        }
                                }

                                await Task.Delay(searchRefreshDelayInSecond * 1000, _cancellationToken.Token);
                        }
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="eventCallback"></param>
                /// <returns></returns>
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
                /// 
                /// </summary>
                /// <returns></returns>
                public aDeviceError UnSubscribeAllDeviceEvents()
                {
                        Dispose();

                        return aDeviceError.Success;
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="devices"></param>
                /// <returns></returns>
                public aDeviceError GetDeviceList(out IEnumerable<string> devices)
                {
                        devices = ConnectedDevices;
                        return aDeviceError.Success;
                }
        }
}