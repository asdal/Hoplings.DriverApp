using Hoplings.DriverApp.ViewModels;

namespace Hoplings.DriverApp.Pages;

public partial class MainPage : ContentPage
{
	int count = 0;

    public MainPage(DriverStatusViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }   
}
