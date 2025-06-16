using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatUserConnectionManager _connectionManager;

        public ChatHub(ChatUserConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User.Claims.FirstOrDefault(c => c.Type == "userName")?.Value;

            if (!string.IsNullOrEmpty(username))
            {
                var connectionId = Context.ConnectionId.ToString();
                _connectionManager.AddUserConnection(username, connectionId);

                await Clients.Others.SendAsync("UpdateOnlineUsers");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Claims.FirstOrDefault(c => c.Type == "userName")?.Value;

            if (!string.IsNullOrEmpty(username))
            {
                _connectionManager.RemoveUserConnection(username);

                await Clients.Others.SendAsync("UpdateOnlineUsers");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public List<string> GetAllUsers() 
        { 
            return _connectionManager.GetOnlineUsers();
        }

        public List<string> GetAllChatInvites()
        {
            var username = Context.User.Claims.FirstOrDefault(c => c.Type == "userName")?.Value;

            return _connectionManager.GetAllChatConnections(username);
        }

        public async Task AcceptChatRequest(string fromUser)
        {
            var username = Context.User.Claims.FirstOrDefault(c => c.Type == "userName")?.Value;

            var result = _connectionManager.AcceptChatConnection(username, fromUser);

            if (result != null)
            {
                await Clients.Caller.SendAsync("ChatAcceptFailed", result);
                return;
            }

            await Clients.Client(_connectionManager.GetChatId(fromUser)).SendAsync("ChatAccept", username);
        }

        public async Task RefuseChatRequest(string fromUser)
        {
            var username = Context.User.Claims.FirstOrDefault(c => c.Type == "userName")?.Value;

            _connectionManager.RefuseChatConnection(username, fromUser);

            
            await Clients.Client(_connectionManager.GetChatId(fromUser))
                    .SendAsync("ChatRefused");
        }

        public async Task CancelChatRequest(string toUser)
        {
            var username = Context.User.Claims.FirstOrDefault(c => c.Type == "userName")?.Value;

            _connectionManager.DeclieveChatConnection(username, toUser);

            await Clients.Client(_connectionManager.GetChatId(toUser))
                 .SendAsync("UpdateChatRequest");
        }
    
        public async Task SendChatRequest(string toUser)
        {
            var username = Context.User.Claims.FirstOrDefault(C => C.Type == "userName")?.Value;

            _connectionManager.SendChatConnection(username, toUser);

            await Clients.Client(_connectionManager.GetChatId(toUser)).SendAsync("UpdateChatRequest");
        }

        public async Task SendMessage(string toUser, string encryptedMessage)
        {
            await Clients.Client(_connectionManager.GetChatId(toUser)).SendAsync("ReceivedMessage", encryptedMessage);
        }
    }
}
