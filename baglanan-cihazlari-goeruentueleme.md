# Bağlanan Cihazları Görüntüleme

Standalone cihazımıza bağlanan bir mobil cihaz olduğunda geriye event döndürmesi için DeviceWatcher sınıfını kullanıyoruz.

## DeviceWatcher

Bu sınıf, Standalone cihazımıza  bir mobil cihaz bağlandığında `deviceAdded`, cihaz bağlantısı kesildiğinde ise `deviceRemoved` eventini çağırır. Bu eventler geriye [DeviceEventArgs](structs/deviceeventargs.md) struct'unu döndürür.

{% hint style="info" %}
DeviceWatcher sınıfı sadece Standalone cihazlarda çalıştırılmalıdır.
{% endhint %}

Öncelikle bağlantıları kontrol edeceğimiz bir sınıf oluşturalım ve `DeviceWatcher` sınıfımızı ekleyelim.

<pre class="language-csharp" data-title="HostScript.cs" data-line-numbers data-full-width="false"><code class="lang-csharp">private DeviceWatcher _deviceWatcher;
<strong>
</strong><strong>private void OnEnable()
</strong>{
    _deviceWatcher = new DeviceWatcher();
    _deviceWatcher.deviceAdded += DeviceWatcher_OnDeviceAdded;
    _deviceWatcher.deviceRemoved += DeviceWatcher_OnDeviceRemoved;
    _deviceWatcher.devicePaired += DeviceWatcher_OnDevicePaired;
    _deviceWatcher.SetEnabled(true);
    Debug.Log("Device watcher running...");
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
</code></pre>

### Eventler

Üst tarafta eklediğimiz eventleri şu şekilde kontrol edebiliriz:

{% code title="HostScript.cs" lineNumbers="true" fullWidth="false" %}
```csharp
private async void DeviceWatcher_OnDeviceAdded(DeviceEventArgs e)
{
    //Give info when new device added
    Debug.Log($"Device added: [{e.deviceInfo.udid}]");

    if (!string.IsNullOrEmpty(_deviceId))
        return;

    //Check device connected with USB
    if (e.deviceInfo.connectionType != DeviceConnectionType.Usbmuxd)
        return;
}

private void DeviceWatcher_OnDeviceRemoved(DeviceEventArgs e)
{
    Debug.Log($"Device removed: [{e.deviceInfo.udid}]");
}

private void DeviceWatcher_OnDevicePaired(DeviceEventArgs e)
{
    Debug.Log($"Device paired:[{e.deviceInfo.udid}]");
}
```
{% endcode %}

