using Hoplings.DriverApp.Services;  // ✅ For HeartbeatService

namespace Hoplings.DriverApp;

public partial class App : Application
{
    private readonly HeartbeatService _heartbeatService;

    public App(HeartbeatService heartbeatService)
    {
        InitializeComponent();

        MainPage = new AppShell();

        // ✅ Save instance to start later
        _heartbeatService = heartbeatService;

        // ✅ Start heartbeat when app launches
        _ = StartHeartbeatAsync();
    }

    private async Task StartHeartbeatAsync()
    {
        try
        {
            await _heartbeatService.StartAsync();
        }
        catch (Exception ex)
        {
            // TODO: Add your logging or handle errors here
            Console.WriteLine($"HeartbeatService error: {ex.Message}");
        }
    }
}
