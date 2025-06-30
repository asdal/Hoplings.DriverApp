using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices.Sensors;

namespace Hoplings.DriverApp.Services
{
    public class HeartbeatService
    {
        private readonly HubConnection _hubConnection;
        private readonly IGeolocation _geolocation;
        private CancellationTokenSource? _cts;

        public HeartbeatService(IGeolocation geolocation)
        {
            _geolocation = geolocation;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://dispatch.hoplings.com/driverLocationHub", options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        var token = await SecureStorage.Default.GetAsync("jwt_token");
                        Console.WriteLine($"🚦 JWT Token: {token}");
                        return token;
                    };
                })
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task StartAsync()
        {
            // If already started, do nothing
            if (_cts != null && !_cts.IsCancellationRequested)
                return;

            _cts = new CancellationTokenSource();
            await _hubConnection.StartAsync();

            _ = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    var location = await _geolocation.GetLastKnownLocationAsync();
                    if (location != null)
                    {
                        await _hubConnection.InvokeAsync(
                            "SendLocationUpdate",
                            location.Latitude,
                            location.Longitude
                        );
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), _cts.Token);
                }
            }, _cts.Token);
        }

        public async Task StopAsync()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.StopAsync();
            }
        }
    }
}
