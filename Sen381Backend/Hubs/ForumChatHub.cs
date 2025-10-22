using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Sen381Backend.Hubs
{
    public class ForumChatHub : Hub
    {
        public async Task JoinForumGroup(int forumId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"forum_{forumId}");
        }

        public async Task LeaveForumGroup(int forumId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"forum_{forumId}");
        }
    }
}
