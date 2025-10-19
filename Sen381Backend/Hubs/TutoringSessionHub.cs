using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Sen381Backend.Hubs
{
    public class TutoringSessionHub : Hub
    {
        // Store user connections by session ID
        private static readonly ConcurrentDictionary<string, List<string>> SessionConnections = new();

        public async Task JoinSession(string sessionId)
        {
            var connectionId = Context.ConnectionId;
            
            // Add user to session group
            await Groups.AddToGroupAsync(connectionId, $"session_{sessionId}");
            
            // Track connections for this session
            SessionConnections.AddOrUpdate($"session_{sessionId}", 
                new List<string> { connectionId },
                (key, existing) => {
                    if (!existing.Contains(connectionId))
                        existing.Add(connectionId);
                    return existing;
                });

            Console.WriteLine($"[TutoringSessionHub] User {connectionId} joined session {sessionId}");
            
            // Notify others in the session that a user joined
            await Clients.Group($"session_{sessionId}").SendAsync("UserJoined", connectionId);
        }

        public async Task LeaveSession(string sessionId)
        {
            var connectionId = Context.ConnectionId;
            
            // Remove user from session group
            await Groups.RemoveFromGroupAsync(connectionId, $"session_{sessionId}");
            
            // Remove from tracking
            if (SessionConnections.TryGetValue($"session_{sessionId}", out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    SessionConnections.TryRemove($"session_{sessionId}", out _);
                }
            }

            Console.WriteLine($"[TutoringSessionHub] User {connectionId} left session {sessionId}");
            
            // Notify others in the session that a user left
            await Clients.Group($"session_{sessionId}").SendAsync("UserLeft", connectionId);
        }

        // WebRTC Signaling Methods
        public async Task SendOffer(string sessionId, string targetConnectionId, object offer)
        {
            Console.WriteLine($"[TutoringSessionHub] Sending offer from {Context.ConnectionId} to {targetConnectionId}");
            await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
        }

        public async Task SendAnswer(string sessionId, string targetConnectionId, object answer)
        {
            Console.WriteLine($"[TutoringSessionHub] Sending answer from {Context.ConnectionId} to {targetConnectionId}");
            await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
        }

        public async Task SendIceCandidate(string sessionId, string targetConnectionId, object candidate)
        {
            Console.WriteLine($"[TutoringSessionHub] Sending ICE candidate from {Context.ConnectionId} to {targetConnectionId}");
            await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
        }

        // Chat functionality
        public async Task SendMessage(string sessionId, string message)
        {
            Console.WriteLine($"[TutoringSessionHub] Message in session {sessionId}: {message}");
            await Clients.Group($"session_{sessionId}").SendAsync("ReceiveMessage", Context.ConnectionId, message);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"[TutoringSessionHub] User {connectionId} disconnected");
            
            // Remove from all sessions
            foreach (var kvp in SessionConnections.ToList())
            {
                if (kvp.Value.Contains(connectionId))
                {
                    await Clients.Group(kvp.Key).SendAsync("UserLeft", connectionId);
                    kvp.Value.Remove(connectionId);
                    if (kvp.Value.Count == 0)
                    {
                        SessionConnections.TryRemove(kvp.Key, out _);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
