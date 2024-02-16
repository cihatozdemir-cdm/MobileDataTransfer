# Bağlı Cihazlarla Veri Alışverişi

Öncelikle hem Host  (Standalone) hem de Device (Mobile) için socket bağlantısı oluşturmamız gerekiyor. Bunun için HostSocketConnection ve DeviceSocketConnection sınıflarını kullanıyoruz.

## Socket Bağlantısı Oluşturma

Host tarafında `CreateForTargetDevice` fonksiyonunu kullanarak hedef cihaza göre gerekli sınıfı oluşturmasını sağlıyoruz.

{% code title="HostScript.cs" lineNumbers="true" %}
```csharp
private async void DeviceWatcher_OnDeviceAdded(DeviceEventArgs e)
{
    //...

    var isConnected = false;
    while (!isConnected && !_cancellationTokenSource.IsCancellationRequested)
    {
        try
        {
            //Create Host Socket for socket connection
            socket = HostSocketConnection.CreateForTargetDevice(e.deviceInfo);
            await socket.ConnectAsync(SocketTextureUtility.Port);
            isConnected = true;
        }
        catch (iDeviceException ex) //Specially exception for IOS
        {
            Debug.Log($"Connection failed due to {ex.ErrorCode}. Trying to connect after {connectionWaitTime} secs...");
        
            isConnected = false;
            socket?.Dispose();
            socket = null;
        }
        catch (Exception ex)
        {
            Debug.Log($"Connection failed due to {ex}. Trying to connect after {connectionWaitTime} secs...");
        
            isConnected = false;
            socket?.Dispose();
            socket = null;
        }

        await Task.Delay((int) (connectionWaitTime * 1000), _cancellationTokenSource.Token);
    }
}
```
{% endcode %}



Aynı şekilde cihaz tarafında da gelen verilerin dinlenmesi için `DeviceSocketConnection` sınıfını oluşturmamız gerekiyor.

<pre class="language-csharp" data-title="DeviceScript.cs" data-line-numbers><code class="lang-csharp">private async void Start()
{
<strong>    using var socket = new DeviceSocketConnection();
</strong>    await socket.ConnectAsync(SocketTextureUtility.Port);
    
    Debug.Log("Connected to host!");
}
</code></pre>



{% hint style="warning" %}
HostScript.cs sınıfının sadece Standalone, DeviceScript.cs sınıfının ise sadece Mobil cihazlarda çalıştırıldığına dikkat etmek gerekmektedir.
{% endhint %}



## Veri Gönderme

Socket bağlantılarımızı oluşturduktan sonra geriye sadece veri göndermek veya almak kalıyor. Bunun için öncelikle Host tarafından nasıl veri gönderilir bunu göreceğiz.

{% hint style="info" %}
İki tarafın anlaştığını anlamak için cihazlar arasında [ACK iletimi](what-is-ack.md) yapacağız.
{% endhint %}

### Host To Device

Öncelikle bilgisayar tarafında rastgele bir sayı oluşturup bunu mobil cihaza iletmek istiyoruz. Bunun için rastgele bir sayı oluşturalım ve socket kullanarak cihazımıza iletelim.

{% code title="HostScript.cs" lineNumbers="true" %}
```csharp
private async void DeviceWatcher_OnDeviceAdded(DeviceEventArgs e)
{
    //...

    var randomNumber = Random.Range(0, 999);
    Debug.Log($"Host says: {randomNumberFromHost}");
    
    await socket.SendInt32Async(randomNumber);
    
    Debug.Log("Waiting for ACK...");
    var ack = await socket.ReceiveInt32Async();
    Debug.Log($"Received  ACK: {(ack.HasValue ? "YES" : "NO")}");
}
```
{% endcode %}



Gönderilen bu veriyi cihaz tarafından almak için tekrar oluşturulan socket'i kullanıyoruz.

{% code title="DeviceScript.cs" lineNumbers="true" %}
```csharp
private async void Start()
{
    //...
    
    var randomNumberFromHost = await socket.ReceiveInt32Async();
    Debug.Log($"Host says: {randomNumberFromHost}");
    
    Debug.Log("Sending ACK...");
    await socket.SendInt32Async(1);
}
```
{% endcode %}

### Device To Host

Şimdi ise bu olayın tersini gerçekleştirmek istiyoruz. Cihaz tarafında oluşturulan rastgele bir sayıyı host tarafında göndermek istiyoruz. Bunun için aynı şekilde socket kullanarak veriyi iletmemiz ve host tarafında veriyi okumamız gerekiyor.

{% code title="DeviceScript.cs" lineNumbers="true" %}
```csharp
private async void Start()
{
    //...
    
    var randomNumber = Random.Range(0, 999);
    Debug.Log($"Device says: {randomNumberFromHost}");
    
    await socket.SendInt32Async(randomNumber);
    
    Debug.Log("Waiting for ACK...");
    var ack = await socket.ReceiveInt32Async();
    
    //Finish data transfer
    socket.Disconnect();
    socket.Dispose();
}
```
{% endcode %}

{% code title="HostScript.cs" lineNumbers="true" %}
```csharp
private async void DeviceWatcher_OnDeviceAdded(DeviceEventArgs e)
{
    //...

    var randomNumberFromHost = await socket.ReceiveInt32Async();
    Debug.Log($"Device says: {randomNumberFromHost}");
    
    Debug.Log("Sending ACK...");
    await socket.SendInt32Async(1);
    Debug.Log("DONE!");
    
    //Finish data transfer
    socket.Disconnect();
    socket.Dispose();
}
```
{% endcode %}



Bu şekilde iki cihaz arasında veri iletimini gerçekleştirmiş olduk.
