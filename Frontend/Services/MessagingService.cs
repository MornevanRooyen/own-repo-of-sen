using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using System.Text.Json;
using Sen381.Business.Models;
using Frontend.Models;

namespace Frontend.Services
{
    public class MessagingService : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly NavigationManager _navigationManager;
        private bool _isConnected = false;

        public event Action<DirectMessage>? MessageReceived;

        public MessagingService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
            
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7228/messagingHub")
                .Build();

            // Handle incoming messages
            _hubConnection.On<SignalRMessageDto>("ReceiveMessage", OnReceiveMessage);
            
            // Add connection state change handlers
            _hubConnection.Closed += async (error) =>
            {
                Console.WriteLine($"[MessagingService] Connection closed: {error?.Message}");
                _isConnected = false;
            };
            
            _hubConnection.Reconnected += async (connectionId) =>
            {
                Console.WriteLine($"[MessagingService] Connection reconnected: {connectionId}");
                _isConnected = true;
            };
            
            _hubConnection.Reconnecting += async (error) =>
            {
                Console.WriteLine($"[MessagingService] Connection reconnecting: {error?.Message}");
                _isConnected = false;
            };
        }

        private void OnReceiveMessage(SignalRMessageDto messageDto)
        {
            try
            {
                Console.WriteLine($"[MessagingService] Received SignalR message: ID {messageDto.MessageId}, Text: {messageDto.MessageText?.Substring(0, Math.Min(50, messageDto.MessageText?.Length ?? 0))}...");
                
                // Convert SignalRMessageDto to DirectMessage
                var message = new DirectMessage
                {
                    Id = messageDto.MessageId,
                    SenderId = messageDto.SenderId,
                    ReceiverId = messageDto.ReceiverId,
                    MessageText = messageDto.MessageText,
                    SentAt = messageDto.SentAt,
                    IsRead = messageDto.IsRead
                };
                
                Console.WriteLine($"[MessagingService] Converted to DirectMessage with ID {message.Id}");
                MessageReceived?.Invoke(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessagingService] Error handling live message: {ex.Message}");
                Console.WriteLine($"[MessagingService] Stack trace: {ex.StackTrace}");
            }
        }

        public async Task StartAsync()
        {
            try
            {
                Console.WriteLine($"[MessagingService] Current connection state: {_hubConnection.State}");
                
                if (_hubConnection.State != HubConnectionState.Connected)
                {
                    Console.WriteLine("[MessagingService] Starting SignalR connection...");
                    await _hubConnection.StartAsync();
                    Console.WriteLine($"[MessagingService] Connection started. New state: {_hubConnection.State}");
                }
                else
                {
                    Console.WriteLine("[MessagingService] SignalR already connected");
                }
                
                _isConnected = true;
                Console.WriteLine("[MessagingService] SignalR connection established successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessagingService] Error starting SignalR: {ex.Message}");
                Console.WriteLine($"[MessagingService] Stack trace: {ex.StackTrace}");
                _isConnected = false;
            }
        }

        public async Task JoinMessagingAsync(string userId)
        {
            if (_isConnected)
            {
                try
                {
                    Console.WriteLine($"[MessagingService] Attempting to join messaging for user {userId}");
                    await _hubConnection.InvokeAsync("JoinMessaging", userId);
                    Console.WriteLine($"[MessagingService] User {userId} joined messaging successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MessagingService] Error joining messaging: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"[MessagingService] Cannot join messaging - not connected. State: {_hubConnection.State}");
            }
        }

        public async Task LeaveMessagingAsync(string userId)
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("LeaveMessaging", userId);
                Console.WriteLine($"[MessagingService] User {userId} left messaging");
            }
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        
        public string ConnectionState => _hubConnection?.State.ToString() ?? "Unknown";

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
