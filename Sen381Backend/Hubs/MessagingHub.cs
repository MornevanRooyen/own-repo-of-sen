using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Sen381Backend.Hubs
{
    public class MessagingHub : Hub
    {
        // Store user connections by user ID
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();

        public async Task JoinMessaging(string userId)
        {
            var connectionId = Context.ConnectionId;
            
            // Store user connection
            UserConnections.AddOrUpdate(userId, connectionId, (key, existing) => connectionId);
            
            Console.WriteLine($"[MessagingHub] User {userId} joined messaging with connection {connectionId}");
            
            // Join a group for this user (useful for notifications)
            await Groups.AddToGroupAsync(connectionId, $"user_{userId}");
        }

        public async Task LeaveMessaging(string userId)
        {
            var connectionId = Context.ConnectionId;
            
            // Remove user connection
            UserConnections.TryRemove(userId, out _);
            
            // Remove from group
            await Groups.RemoveFromGroupAsync(connectionId, $"user_{userId}");
            
            Console.WriteLine($"[MessagingHub] User {userId} left messaging");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"[MessagingHub] User with connection {connectionId} disconnected");
            
            // Remove user from connections
            var userIdToRemove = UserConnections.FirstOrDefault(x => x.Value == connectionId).Key;
            if (userIdToRemove != null)
            {
                UserConnections.TryRemove(userIdToRemove, out _);
                await Groups.RemoveFromGroupAsync(connectionId, $"user_{userIdToRemove}");
                Console.WriteLine($"[MessagingHub] Removed user {userIdToRemove} from connections");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Helper method to get connection ID for a user
        public static string? GetUserConnectionId(string userId)
        {
            UserConnections.TryGetValue(userId, out var connectionId);
            return connectionId;
        }

        // Helper method to check if user is online
        public static bool IsUserOnline(string userId)
        {
            return UserConnections.ContainsKey(userId);
        }
    }
}
