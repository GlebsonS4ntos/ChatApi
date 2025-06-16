using ChatCore.Services;
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

                await Clients.Others.SendAsync("NewUser");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Claims.FirstOrDefault(c => c.Type == "userName")?.Value;

            if (!string.IsNullOrEmpty(username))
            {
                _connectionManager.RemoveUserConnection(username);

                await Clients.Others.SendAsync("DisconectUser");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
