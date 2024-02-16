---
layout:
  title:
    visible: true
  description:
    visible: true
  tableOfContents:
    visible: true
  outline:
    visible: true
  pagination:
    visible: true
---

# DeviceEventArgs

Herhangi bir cihazda bağlantı değişikliği olduğunda döndürülen sınıftır. Bu sınıf geriye [**DeviceInfo**](deviceinfo.md) sınıfını döndürür.

{% code lineNumbers="true" %}
```csharp
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
```
{% endcode %}
