using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;


namespace Hoplings.DriverApp.Services
{
    /// <summary>
    /// This service keeps your driver online:
    ///  - Connects to the backend SignalR hub.
    ///  - Sends the driver’s GPS location every few seconds.
    ///  - Stops cleanly when the driver goes offline.
    ///  Debug.WriteLine is used throughout to help trace each step.
    /// </summary>
    public class HeartbeatService
    {
        public HubConnectionState ConnectionState => _hubConnection?.State ?? HubConnectionState.Disconnected;

        // Talks to backend using SignalR
        private readonly HubConnection _hubConnection;

        // Gets phone GPS location
        private readonly IGeolocation _geolocation;

        // Controls stopping the background loop
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Sets up the SignalR connection with JWT authentication and geolocation.
        /// </summary>
        public HeartbeatService(IGeolocation geolocation)
        {
            _geolocation = geolocation;

            string _apiBaseUrl = "https://dispatch.hoplings.com/DriverLocationHub";

            try
            {
                Debug.WriteLine("🔧 Starting HubConnectionBuilder...");
                Debug.WriteLine($"🌍 Hub URL: {_apiBaseUrl}");

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{_apiBaseUrl}", options =>
                    {


                        options.AccessTokenProvider = async () =>
                        {
                            Debug.WriteLine("🔑 AccessTokenProvider: Fetching JWT token from SecureStorage...");
                            var token = await SecureStorage.Default.GetAsync("jwt_token");
                            Debug.WriteLine($"✅ AccessTokenProvider: Token = {token}");

                            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

                            Debug.WriteLine($"Token Issued: {jwtToken.IssuedAt}");
                            Debug.WriteLine($"Valid From: {jwtToken.ValidFrom}");
                            Debug.WriteLine($"Valid To: {jwtToken.ValidTo}");
                            Debug.WriteLine($"Current UTC: {DateTime.UtcNow}");
                            return token;
                        };
                    })
                    .WithAutomaticReconnect()
                    .Build();

                Debug.WriteLine("✅ HubConnection built successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ ------------------------------------");
                Debug.WriteLine($"❌ Exception while building HubConnection:");
                Debug.WriteLine($"❌ Message: {ex.Message}");
                Debug.WriteLine($"❌ Type: {ex.GetType()}");
                Debug.WriteLine($"❌ StackTrace:\n{ex.StackTrace}");
                Debug.WriteLine("❌ ------------------------------------");
                throw; // Optional: rethrow if you want it to bubble up
            }
        }

        /// <summary>
        /// Starts sending location:
        ///  - Connects to SignalR hub.
        ///  - Starts sending GPS every 10 sec.
        /// </summary>
        public async Task StartAsync()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                Debug.WriteLine("ℹ️ Heartbeat already running.");
                return;
            }

            _cts = new CancellationTokenSource();

            try
            {
                Debug.WriteLine("🔌 Attempting to start SignalR connection...");
                await _hubConnection.StartAsync();
                Debug.WriteLine("✅ SignalR connection established.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to start SignalR connection:");
                Debug.WriteLine($"❌ ------------------------------------");
                Debug.WriteLine($"❌ Exception: {ex.Message}");
                Debug.WriteLine($"❌ ------------------------------------");
                Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"❌ ------------------------------------");

                    Debug.WriteLine($"❌ InnerException: {ex.InnerException.Message}");
                    Debug.WriteLine($"❌ InnerException StackTrace: {ex.InnerException.StackTrace}");
                }

                return;
            }

            _ = Task.Run(async () =>
            {
                Debug.WriteLine("▶️ Heartbeat loop started. Sending GPS every 10 sec.");
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        var location = await _geolocation.GetLastKnownLocationAsync();

                        if (location != null)
                        {
                            await _hubConnection.InvokeAsync(
                                "SendLocationUpdate",
                                location.Latitude,
                                location.Longitude
                            );

                            Debug.WriteLine($"📡 Sent location: {location.Latitude}, {location.Longitude}");
                        }
                        else
                        {
                            Debug.WriteLine("⚠️ No last known location available.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"⚠️ Error during location send: {ex.Message}");
                    }

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), _cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        Debug.WriteLine("⏹️ Heartbeat loop cancelled.");
                        break;
                    }
                }
            }, _cts.Token);
        }

        /// <summary>
        /// Stops sending location:
        ///  - Cancels the loop.
        ///  - Closes SignalR connection.
        /// </summary>
        public async Task StopAsync()
        {
            try
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                    Debug.WriteLine("🛑 Heartbeat loop stopped.");
                }

                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.StopAsync();
                    Debug.WriteLine("🔌 SignalR connection closed.");
                }
                else
                {
                    Debug.WriteLine("ℹ️ SignalR connection already stopped or not started.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error stopping HeartbeatService: {ex.Message}");
            }
        }
    }
}
