namespace ChatCore.Services
{
    public class ChatUserConnectionManager
    {
        private readonly Dictionary<string, string> _userConnections = new();

        public void AddUserConnection(string username, string connectionId)
        {
            _userConnections[username] = connectionId;
        }

        public void RemoveUserConnection(string username)
        {
            _userConnections.Remove(username);
        }
    }
}