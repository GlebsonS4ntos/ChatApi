using System.Collections.Concurrent;

public class ChatUserConnectionManager
{
    private readonly ConcurrentDictionary<string, string> _userConnections = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _sendChatConnection = new();
    private readonly HashSet<(string, string)> _chatConnections = new();
    private readonly object _sendLock = new();
    private readonly object _chatLock = new();

    public void AddUserConnection(string username, string connectionId)
    {
        _userConnections[username] = connectionId;
    }

    public void RemoveUserConnection(string username)
    {
        lock (_chatLock)
        {
            _chatConnections.RemoveWhere(t => t.Item1 == username || t.Item2 == username);
        }

        lock (_sendLock)
        {
            _sendChatConnection.TryRemove(username, out _);
            foreach (var entry in _sendChatConnection.Values)
            {
                entry.Remove(username);
            }
        }

        _userConnections.TryRemove(username, out _);
    }

    public string? AcceptChatConnection(string username, string toUser)
    {
        lock (_sendLock)
        {
            if (!_sendChatConnection.TryGetValue(username, out var requests) || !requests.Contains(toUser))
                return "O pedido para o chat não existe mais.";

            _sendChatConnection[toUser].Remove(username);
        }

        lock (_chatLock)
        {
            _chatConnections.RemoveWhere(t =>
                (t.Item1 == username || t.Item2 == username) ||
                (t.Item1 == toUser || t.Item2 == toUser)
            );

            _chatConnections.Add((username, toUser));
        }

        return null;
    }

    public void DeclieveChatConnection(string username, string toUser)
    {
        lock (_sendLock)
        {
            _sendChatConnection[toUser].Remove(username);
        }
    }

    public void RefuseChatConnection(string username, string toUser)
    {
        DeclieveChatConnection(username, toUser);
    }

    public void SendChatConnection(string username, string toUser)
    {
        lock (_sendLock)
        {
            if (!_sendChatConnection.ContainsKey(toUser))
                _sendChatConnection[toUser] = new HashSet<string>();

            _sendChatConnection[toUser].Add(username);
        }
    }

    public bool AreInChat(string user1, string user2)
    {
        lock (_chatLock)
        {
            return _chatConnections.Contains((user1, user2)) || _chatConnections.Contains((user2, user1));
        }
    }

    public List<string> GetOnlineUsers()
    {
        return _userConnections.Keys.ToList();
    }

    public List<string> GetAllChatConnections(string username)
    {
        lock (_sendLock)
        {
            return _sendChatConnection[username].ToList();
        }
    }
}
