using Hoplings.DriverApp.ViewModels;

namespace Hoplings.DriverApp.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
