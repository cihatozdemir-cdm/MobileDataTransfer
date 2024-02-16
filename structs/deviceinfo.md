---
description: Bu struct içerisinde cihaz ile alakalı bazı bilgileri bulundurur.
---

# DeviceInfo

{% code lineNumbers="true" %}
```csharp
/// <summary>
/// UDID of the device.
/// </summary>
public string udid { get; }

/// <summary>
/// Name of the device.
/// </summary>
public string name { get; }

/// <summary>
/// Type of the device. (Android or IOS)
/// </summary>
public DeviceType deviceType { get; }

/// <summary>
/// Connection type of the device.
/// </summary>
public DeviceConnectionType connectionType { get; }
```
{% endcode %}
