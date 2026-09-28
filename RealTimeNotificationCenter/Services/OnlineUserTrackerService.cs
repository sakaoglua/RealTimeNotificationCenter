using System.Collections.Concurrent;

namespace RealTimeNotificationCenter.Services;

public enum UserConnectionStatus
{
    FirstConnection,
    Reconnected,
    AdditionalConnection
}

public class OnlineUserTrackerService
{
    private readonly ConcurrentDictionary<string, string> _connections = new();

    private readonly ConcurrentDictionary<string, CancellationTokenSource> _pendingDisconnects = new(StringComparer.OrdinalIgnoreCase);

    public UserConnectionStatus AddUser(string connectionId, string userName)
    {
        var hasActiveConnection = _connections.Values.Any(existingUserName => string.Equals(existingUserName, userName, StringComparison.OrdinalIgnoreCase));
        var wasPendingDisconnect = _pendingDisconnects.TryRemove(userName, out var pendingCancellation);

        if (wasPendingDisconnect)
        {
            pendingCancellation?.Cancel();
            pendingCancellation?.Dispose();
        }

        _connections[connectionId] = userName;

        if (wasPendingDisconnect)
        {
            return UserConnectionStatus.Reconnected;
        }

        if (hasActiveConnection)
        {
            return UserConnectionStatus.AdditionalConnection;
        }

        return UserConnectionStatus.FirstConnection;
    }

    public string? RemoveConnection(string connectionId)
    {
        return _connections.TryRemove(connectionId, out var userName) ? userName : null;
    }

    public bool HasActiveConnection(string userName)
    {
        return _connections.Values.Any(existingUserName => string.Equals(existingUserName, userName, StringComparison.OrdinalIgnoreCase));
    }

    public CancellationToken BeginDisconnectCountdown(string userName)
    {
        var cancellationTokenSource = new CancellationTokenSource();

        _pendingDisconnects.AddOrUpdate(userName, cancellationTokenSource, (_, existingCancellation) =>
        {
            existingCancellation.Cancel();
            existingCancellation.Dispose();

            return cancellationTokenSource;
        });

        return cancellationTokenSource.Token;
    }

    public bool CompleteDisconnect(string userName)
    {
        if (HasActiveConnection(userName))
        {
            return false;
        }

        if (_pendingDisconnects.TryRemove(userName, out var cancellationTokenSource))
        {
            cancellationTokenSource.Dispose();
            return true;
        }

        return false;
    }

    public List<string> GetOnlineUsers()
    {
        var activeUsers = _connections.Values;

        var pendingUsers = _pendingDisconnects.Keys;

        return activeUsers
            .Concat(pendingUsers)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(userName => userName)
            .ToList();
    }

    public List<string> GetConnectionIds(string userName) // Kullanıcının tüm aktif bağlantılarını almak için 
    {
        return _connections
            .Where(x => string.Equals(x.Value,userName,StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Key)
            .ToList();
    }
}