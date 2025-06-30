using Hoplings.DriverApp.Pages;

namespace Hoplings.DriverApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("MainPage", typeof(MainPage));
    }
}
