using Hoplings.DriverApp.Pages;
using Hoplings.DriverApp.Services; // ✅ Add this for HeartbeatService
using Microsoft.Extensions.Logging;
using Hoplings.DriverApp.ViewModels;
using Hoplings.DriverApp.Pages; // if you need it too
using Microsoft.Maui.Devices.Sensors;

namespace Hoplings.DriverApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<DriverStatusViewModel>();
        builder.Services.AddSingleton<MainPage>(); // if you use it!


        // ✅ Register platform geolocation service
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);

        // ✅ Register your HeartbeatService
        builder.Services.AddSingleton<HeartbeatService>();

        return builder.Build();
    }
}
