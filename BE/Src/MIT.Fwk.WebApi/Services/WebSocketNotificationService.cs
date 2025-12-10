using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Core.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Services
{
    /// <summary>
    /// Manages WebSocket connections for real-time notifications.
    /// Runs as a background service and handles client connections, message broadcasting, and connection cleanup.
    /// </summary>
    public class WebSocketNotificationService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly WebSocketOptions _webSocketOptions;

        private readonly ConcurrentDictionary<string, WebSocketConnection> _activeConnections = new();
        private readonly Timer _cleanupTimer;

        // Configuration constants
        private const int ConnectionTimeoutMinutes = 5;
        private const int CleanupIntervalSeconds = 30;
        private const int MaxPayloadSizeBytes = 1024 * 100; // 100 KB
        private const int BufferSize = 1024 * 10; // 10 KB

        // Error messages
        private const string ErrorInvalidInput = "InputString not correct. Closing connection due to security policy.";
        private const string ErrorMaxPayloadExceeded = "Payload size exceeded. Maximum allowed: {0} bytes.";
        private const string SuccessConnected = "Successfully connected as {0}";
        private const string SuccessSent = "Sended successfully!";

        public WebSocketNotificationService(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            IOptions<WebSocketOptions> webSocketOptions)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _webSocketOptions = webSocketOptions.Value;

            // Initialize cleanup timer
            _cleanupTimer = new Timer(CleanupStaleConnections, null, TimeSpan.FromSeconds(CleanupIntervalSeconds), TimeSpan.FromSeconds(CleanupIntervalSeconds));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create scope for logging throughout ExecuteAsync lifetime
            using var scope = _scopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();

            var listener = new HttpListener();
            string launchUrl = _webSocketOptions.LaunchUrl;

            try
            {
                listener.Prefixes.Add(launchUrl);
                listener.Start();

                logService.Info($"WebSocket listening on {launchUrl.Replace("*", "0.0.0.0")}", "WebSocketService");
            }
            catch (Exception ex)
            {
                logService.Error($"Failed to start WebSocket listener on {launchUrl}. Error: {ex.Message}", "WebSocketService");
                return;
            }

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Accept new connection
                    var listenerContext = await listener.GetContextAsync();

                    if (listenerContext.Request.IsWebSocketRequest)
                    {
                        // Fire and forget - handle session in background
                        _ = HandleWebSocketSessionAsync(listenerContext, stoppingToken);
                    }
                    else
                    {
                        listenerContext.Response.StatusCode = 400;
                        listenerContext.Response.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                logService.Error($"Critical error in WebSocket listener loop. Error: {ex.Message}", "WebSocketService");
            }
            finally
            {
                listener.Stop();
                listener.Close();
                logService.Info("WebSocket listener stopped", "WebSocketService");
            }
        }

        private async Task HandleWebSocketSessionAsync(HttpListenerContext listenerContext, CancellationToken cancellationToken)
        {
            // Create scope for this session
            using var scope = _scopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();

            WebSocketContext webSocketContext = null;
            WebSocket webSocket = null;
            string clientId = "";

            try
            {
                webSocketContext = await listenerContext.AcceptWebSocketAsync(null);
                webSocket = webSocketContext.WebSocket;

                // Step 1: Handshake - Read client ID
                var handshakeData = await ReceiveMessageAsync(webSocket, logService, cancellationToken);

                if (handshakeData == null)
                {
                    await CloseWithErrorAsync(webSocket, ErrorInvalidInput, logService, cancellationToken);
                    return;
                }

                try
                {
                    var json = JObject.Parse(handshakeData);
                    string userId = json["userId"]?.ToString();
                    string fingerPrint = json["fingerprint"]?.ToString();

                    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(fingerPrint))
                    {
                        await CloseWithErrorAsync(webSocket, ErrorInvalidInput, logService, cancellationToken);
                        return;
                    }

                    clientId = $"{userId}|{fingerPrint}";
                }
                catch (Exception ex)
                {
                    logService.Error($"Failed to parse handshake JSON. Error: {ex.Message}", "WebSocketService");
                    await CloseWithErrorAsync(webSocket, ErrorInvalidInput, logService, cancellationToken);
                    return;
                }

                // Step 2: Close existing connection for same client (if any)
                if (_activeConnections.TryRemove(clientId, out var existingConnection))
                {
                    await CloseConnectionSafelyAsync(existingConnection.WebSocket);
                }

                // Step 3: Register new connection
                var newConnection = new WebSocketConnection
                {
                    ClientId = clientId,
                    WebSocket = webSocket,
                    LastActivity = DateTime.UtcNow
                };

                _activeConnections[clientId] = newConnection;

                // Send success response
                await SendMessageAsync(webSocket, string.Format(SuccessConnected, clientId), logService, cancellationToken);

                logService.Info($"Client connected: {clientId}", "WebSocketService");

                // Step 4: Message loop
                await HandleMessageLoopAsync(webSocket, clientId, logService, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logService.Info($"WebSocket session cancelled for client {clientId}", "WebSocketService");
            }
            catch (Exception ex)
            {
                logService.Error($"WebSocket exception for client {clientId}. Error: {ex.Message}", "WebSocketService");
            }
            finally
            {
                // Cleanup connection
                if (!string.IsNullOrEmpty(clientId))
                {
                    _activeConnections.TryRemove(clientId, out _);
                }

                if (webSocket != null && webSocket.State != WebSocketState.Closed)
                {
                    await CloseConnectionSafelyAsync(webSocket);
                }

                logService.Info($"Client disconnected: {clientId}", "WebSocketService");
            }
        }

        private async Task HandleMessageLoopAsync(WebSocket webSocket, string clientId, ILogService logService, CancellationToken cancellationToken)
        {
            while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var message = await ReceiveMessageAsync(webSocket, logService, cancellationToken);

                if (message == null)
                {
                    // Connection closed by client
                    break;
                }

                // Update last activity
                if (_activeConnections.TryGetValue(clientId, out var connection))
                {
                    connection.LastActivity = DateTime.UtcNow;
                }

                // Parse and broadcast message
                try
                {
                    var json = JObject.Parse(message);
                    var targetIdsStr = json["ids"]?.ToString();
                    var broadcastMessage = json["message"]?.ToString();

                    if (string.IsNullOrEmpty(targetIdsStr) || string.IsNullOrEmpty(broadcastMessage))
                    {
                        await CloseWithErrorAsync(webSocket, ErrorInvalidInput, logService, cancellationToken);
                        break;
                    }

                    // Broadcast to target clients (optimized with HashSet)
                    var targetIds = targetIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
                    await BroadcastMessageAsync(targetIds, broadcastMessage, logService, cancellationToken);

                    // Send confirmation
                    await SendMessageAsync(webSocket, SuccessSent, logService, cancellationToken);

                    logService.Info($"Message broadcasted to {targetIds.Count} targets by {clientId}", "WebSocketService");
                }
                catch (Exception ex)
                {
                    logService.Error($"Failed to parse or broadcast message from {clientId}. Error: {ex.Message}", "WebSocketService");
                    await CloseWithErrorAsync(webSocket, ErrorInvalidInput, logService, cancellationToken);
                    break;
                }
            }
        }

        private async Task BroadcastMessageAsync(HashSet<string> targetIds, string message, ILogService logService, CancellationToken cancellationToken)
        {
            if (targetIds == null || targetIds.Count == 0)
                return;

            // Pre-convert message to bytes once
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(messageBytes);

            // Parallel broadcast to all target clients
            var sendTasks = _activeConnections
                .Where(kvp => targetIds.Contains(kvp.Key) && kvp.Value.WebSocket.State == WebSocketState.Open)
                .Select(async kvp =>
                {
                    try
                    {
                        await kvp.Value.WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
                        kvp.Value.LastActivity = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        logService.Error($"Failed to send message to client {kvp.Key}. Error: {ex.Message}", "WebSocketService");
                        // Remove failed connection
                        _activeConnections.TryRemove(kvp.Key, out _);
                    }
                });

            await Task.WhenAll(sendTasks);
        }

        private async Task<string> ReceiveMessageAsync(WebSocket webSocket, ILogService logService, CancellationToken cancellationToken)
        {
            try
            {
                var buffer = new byte[BufferSize];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                    return null;
                }

                if (result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                // Validate payload size
                if (result.Count > MaxPayloadSizeBytes)
                {
                    logService.Error($"Payload size exceeded: {result.Count} bytes", "WebSocketService");
                    return null;
                }

                return Encoding.UTF8.GetString(buffer, 0, result.Count);
            }
            catch (Exception ex)
            {
                logService.Error($"Error receiving WebSocket message. Error: {ex.Message}", "WebSocketService");
                return null;
            }
        }

        private async Task SendMessageAsync(WebSocket webSocket, string message, ILogService logService, CancellationToken cancellationToken)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
            }
            catch (Exception ex)
            {
                logService.Error($"Error sending WebSocket message. Error: {ex.Message}", "WebSocketService");
            }
        }

        private async Task CloseWithErrorAsync(WebSocket webSocket, string errorMessage, ILogService logService, CancellationToken cancellationToken)
        {
            try
            {
                await SendMessageAsync(webSocket, errorMessage, logService, cancellationToken);
                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "", cancellationToken);
            }
            catch (Exception ex)
            {
                logService.Error($"Error closing WebSocket with error message. Error: {ex.Message}", "WebSocketService");
            }
        }

        private async Task CloseConnectionSafelyAsync(WebSocket webSocket)
        {
            try
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                else
                {
                    webSocket.Abort();
                }
            }
            catch
            {
                webSocket.Abort();
            }
        }

        private void CleanupStaleConnections(object state)
        {
            // Create scope for logging in timer callback
            using var scope = _scopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();

            try
            {
                var now = DateTime.UtcNow;
                var timeout = TimeSpan.FromMinutes(ConnectionTimeoutMinutes);

                var staleConnections = _activeConnections
                    .Where(kvp => now - kvp.Value.LastActivity > timeout || kvp.Value.WebSocket.State != WebSocketState.Open)
                    .ToList();

                foreach (var staleConnection in staleConnections)
                {
                    if (_activeConnections.TryRemove(staleConnection.Key, out var connection))
                    {
                        _ = CloseConnectionSafelyAsync(connection.WebSocket);
                        logService.Info($"Cleaned up stale connection: {staleConnection.Key}", "WebSocketService");
                    }
                }

                if (staleConnections.Any())
                {
                    logService.Info($"Cleaned up {staleConnections.Count} stale connections. Active: {_activeConnections.Count}", "WebSocketService");
                }
            }
            catch (Exception ex)
            {
                logService.Error($"Error during connection cleanup. Error: {ex.Message}", "WebSocketService");
            }
        }

        public override void Dispose()
        {
            _cleanupTimer?.Dispose();

            // Close all active connections
            foreach (var connection in _activeConnections.Values)
            {
                _ = CloseConnectionSafelyAsync(connection.WebSocket);
            }

            _activeConnections.Clear();

            base.Dispose();
        }

        #region Internal Classes

        private class WebSocketConnection
        {
            public string ClientId { get; set; }
            public WebSocket WebSocket { get; set; }
            public DateTime LastActivity { get; set; }
        }

        #endregion
    }
}
