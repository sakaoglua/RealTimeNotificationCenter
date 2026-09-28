# RealTimeNotificationCenter

ASP.NET Core MVC ve SignalR ile gerçek zamanlı bildirimler gönderen .NET 8 uygulaması.

## Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (SQL Server Express veya Developer sürümü kullanılabilir)
- Migration komutlarını çalıştırmak için `dotnet-ef` aracı

## Kurulum

1. Depoyu klonlayıp proje kök dizinine geçin:

   ```bash
   git clone <depo-adresi>
   cd RealTimeNotificationCenter
   ```

2. `RealTimeNotificationCenter/appsettings.json` dosyasındaki `ConnectionStrings:SqlServer` değerini kendi SQL Server bağlantı bilginize göre düzenleyin. Örnek:

   ```json
   {
     "ConnectionStrings": {
       "SqlServer": "Server=localhost;Database=RealTimeNotificationCenter;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

   Bağlantı dizesini yerel ortamınıza göre ayarlayın. Gerçek kimlik bilgilerini veya sırları kaynak denetimine eklemeyin; gizli değerleri yerel yapılandırmada ya da güvenli bir sır yönetimi çözümünde saklayın.

3. Depoda şu anda EF Core migration dosyaları bulunmuyor. `dotnet-ef` yüklü değilse, .NET EF araçlarının 8.0 sürümünü yükleyin:

   ```bash
   dotnet tool install --global dotnet-ef --version 8.0.0
   ```

   Proje kök dizininden ilk migration'ı oluşturup veritabanını hazırlayın:

   ```bash
   dotnet ef migrations add InitialCreate --project RealTimeNotificationCenter --startup-project RealTimeNotificationCenter
   dotnet ef database update --project RealTimeNotificationCenter --startup-project RealTimeNotificationCenter
   ```

4. Uygulamayı çalıştırın:

   ```bash
   dotnet run --project RealTimeNotificationCenter
   ```

   Geliştirme profilleri uygulamayı `http://localhost:5158` veya `https://localhost:7007` adreslerinde başlatır.

## Visual Studio ile çalıştırma

1. Proje kökündeki `RealTimeNotificationCenter.sln` çözümünü Visual Studio'da açın.
2. `RealTimeNotificationCenter` projesini başlangıç projesi olarak seçin.
3. `appsettings.json` içindeki SQL Server bağlantı dizesini yapılandırın ve yukarıdaki migration komutlarını Paket Yöneticisi Konsolu'ndan veya proje kökünde bir terminalden çalıştırın.
4. `http` ya da `https` başlatma profilini seçip uygulamayı çalıştırın. Geliştirme ortamında Swagger arayüzü otomatik açılır.

## Sayfalar ve API

- `/User/SignUp`: hesap oluşturma
- `/User/SignIn`: oturum açma
- `/swagger`: Swagger/OpenAPI arayüzü; yalnızca `Development` ortamında kullanılabilir

Bildirim HTTP API'si ve SignalR hub'ı oturum açmış kullanıcı gerektirir. SignalR hub'ı `/notificationHub` adresindedir. İstemciler hub üzerinden `JoinGroup(groupName)` ve `LeaveGroup(groupName)` metotlarını çağırarak gruplara katılabilir veya ayrılabilir.

### HTTP bildirim uç noktaları

İsteklerde oturum açmış kullanıcının kimlik doğrulama çerezini kullanın. İstek gövdeleri JSON biçimindedir:

- `POST /api/Notification/SendNotification` — tüm istemcilere bildirim gönderir. Gövde `Title` ve `Message` alanlarını içerir; gönderen kullanıcı adı sunucu tarafından belirlenir.

  ```json
  {
    "Title": "Duyuru",
    "Message": "Yeni bir bildirim."
  }
  ```

- `POST /api/Notification/SendPrivateNotification` — çevrimiçi bir kullanıcıya özel bildirim gönderir. Gövde `UserName`, `Title` ve `Message` alanlarını içerir.

  ```json
  {
    "UserName": "kullanici",
    "Title": "Merhaba",
    "Message": "Size özel bir bildirim."
  }
  ```

- `POST /api/Notification/SendGroupNotification` — belirtilen gruba bildirim gönderir. Gövde `GroupName`, `Title` ve `Message` alanlarını içerir.

  ```json
  {
    "GroupName": "duyurular",
    "Title": "Grup duyurusu",
    "Message": "Gruba gönderilen bildirim."
  }
  ```
