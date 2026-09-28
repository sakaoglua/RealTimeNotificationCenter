using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealTimeNotificationCenter.Services;

namespace RealTimeNotificationCenter.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly OnlineUserTrackerService _onlineUserTracker;

    public NotificationHub(OnlineUserTrackerService onlineUserTracker)
    {
        _onlineUserTracker = onlineUserTracker;
    }

    public override async Task OnConnectedAsync() // Kullanıcı bağlandığında çalışacak metod
    {
        var userName = GetUserName();

        var connectionStatus = _onlineUserTracker.AddUser(Context.ConnectionId,userName); // Kullanıcıyı online kullanıcı listesine ekliyoruz.

        await SendOnlineUsers();

        switch (connectionStatus)
        {
            case UserConnectionStatus.FirstConnection:

                await Clients.Others.SendAsync( // İlk kez bağlanan kullanıcıyı diğer clientlara bildiriyoruz.
                    "UserConnected",
                    userName);

                break;

            case UserConnectionStatus.Reconnected:

                await Clients.Others.SendAsync( // Yeniden bağlanan kullanıcıyı diğer clientlara bildiriyoruz.
                    "UserReconnected",
                    userName);

                break;

            case UserConnectionStatus.AdditionalConnection:

                // Kullanıcı başka bir sekmede zaten bağlı.
                // Tekrar bildirim göndermiyoruz.
                break;
        }

        Console.WriteLine(
            $"{userName} bağlandı. ConnectionId: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception) // Kullanıcı bağlantıyı kestiğinde çalışacak metod
    {
        var userName = _onlineUserTracker.RemoveConnection(Context.ConnectionId);

        if (string.IsNullOrWhiteSpace(userName))
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }

        // Aynı kullanıcı başka bir sekmede hâlâ bağlıysa
        // offline sürecini başlatma.
        if (_onlineUserTracker.HasActiveConnection(userName))
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }

        var cancellationToken = _onlineUserTracker.BeginDisconnectCountdown(userName);

        try
        {
            // Kullanıcıya geri dönmesi için 3 saniye veriyoruz.
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

            var isCompletelyOffline = _onlineUserTracker.CompleteDisconnect(userName);

            if (isCompletelyOffline)
            {
                await SendOnlineUsers();

                await Clients.All.SendAsync("UserDisconnected", userName); // Bütün clientlara kullanıcı çevrimdışı olduğunu bildiriyoruz.

                Console.WriteLine($"{userName} çevrimdışı oldu.");
            }
        }
        catch (TaskCanceledException)
        {
            // Kullanıcı 3 saniye dolmadan yeniden bağlandı.
            // OnConnectedAsync içindeki UserReconnected olayı çalışacak.
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupName) // Kullanıcı gruba katılmak istediğinde çalışacak metod
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            throw new HubException("Grup adı zorunludur.");
        }

        var normalizedGroupName = groupName.Trim();
        var userName = GetUserName();

        await Groups.AddToGroupAsync(Context.ConnectionId, normalizedGroupName); // Kullanıcıyı gruba ekler.

        await Clients.Group(normalizedGroupName).SendAsync("UserJoinedGroup", userName, normalizedGroupName); // İlgili gruba mesaj yollar.

        Console.WriteLine($"{userName}, {normalizedGroupName} grubuna katıldı.");
    }

    public async Task LeaveGroup(string groupName) // Kullanıcı gruptan ayrılmak istediğinde çalışacak metod
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            throw new HubException("Grup adı zorunludur.");
        }

        var normalizedGroupName = groupName.Trim();
        var userName = GetUserName();

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, normalizedGroupName); // Kullanıcıyı gruptan çıkartır.

        await Clients.Group(normalizedGroupName).SendAsync("UserLeftGroup", userName, normalizedGroupName); // İlgili gruba mesaj yollar.

        Console.WriteLine($"{userName}, {normalizedGroupName} grubundan ayrıldı.");
    }

    private string GetUserName()
    {
        var userName = Context.User?.Identity?.Name?.Trim();

        return string.IsNullOrWhiteSpace(userName)
            ? $"Misafir-{Context.ConnectionId[..5]}"
            : userName;
    }

    private async Task SendOnlineUsers() // Kullanıcıların online durumunu güncellemek için çalışacak metod
    {
        var onlineUsers = _onlineUserTracker.GetOnlineUsers();

        await Clients.All.SendAsync("OnlineUsersUpdated", onlineUsers); // Bütün clientlara online kullanıcıları gönderiyoruz.
    }
}