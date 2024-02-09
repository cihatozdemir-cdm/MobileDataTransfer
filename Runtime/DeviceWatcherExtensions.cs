#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using AndroidLib.Unity;
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using UnityEngine;

namespace Cdm.MobileDataTransfer.Extensions
{
    public static class DeviceWatcherExtensions
    {
        /// <summary>
        /// Get the name of the device using the DeviceInfo structure.
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <param name="label">Handshake Label</param>
        /// <returns></returns>
        public static bool PopulateDeviceName(this DeviceInfo deviceInfo, string label = "")
        {
            switch (deviceInfo.deviceType)
            {
                case DeviceType.Android:
                    return PopulateAndroidDeviceName(ref deviceInfo);
                case DeviceType.IOS:
                    return PopulateIOSDeviceName(ref deviceInfo, label);
                case DeviceType.Unknown:
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Get the name of the *iOS* device using the DeviceInfo structure.
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <param name="label">Handshake Label</param>
        /// <returns></returns>
        private static bool PopulateIOSDeviceName(ref DeviceInfo deviceInfo, string label)
        {
            iDeviceHandle deviceHandle = null;
            LockdownClientHandle lockdownClientHandle = null;

            try
            {
                var deviceApi = LibiMobileDevice.Instance.iDevice;
                var lockdownApi = LibiMobileDevice.Instance.Lockdown;

                deviceApi.idevice_new(out deviceHandle, deviceInfo.udid)
                    .ThrowOnError();

                lockdownApi.lockdownd_client_new_with_handshake(deviceHandle, out lockdownClientHandle, label)
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
        
        /// <summary>
        /// Get the name of the *Android* device using the DeviceInfo structure.
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
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
    }
}
#endif