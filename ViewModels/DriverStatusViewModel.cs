using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hoplings.DriverApp.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.Windows.Input;

namespace Hoplings.DriverApp.ViewModels
{
    public partial class DriverStatusViewModel : ObservableObject
    {
        private readonly HeartbeatService _heartbeatService;

        [ObservableProperty]
        private bool isOnline;

        [ObservableProperty]
        private string hubConnectionState;

        public IAsyncRelayCommand ToggleAvailabilityCommand { get; }
        public IRelayCommand CheckConnectionStateCommand { get; }


        public DriverStatusViewModel(HeartbeatService heartbeatService)
        { 
            _heartbeatService = heartbeatService;
            ToggleAvailabilityCommand = new AsyncRelayCommand(ToggleAvailabilityAsync);
            CheckConnectionStateCommand = new RelayCommand(CheckConnectionState);

        }

        private async Task ToggleAvailabilityAsync()
        {
            if (IsOnline)
            {
                await _heartbeatService.StopAsync();
                IsOnline = false;
            }
            else
            {
                await _heartbeatService.StartAsync();
                IsOnline = true;
            }
        }
        private void CheckConnectionState()
        {
            var state = _heartbeatService.ConnectionState;
            Debug.WriteLine($"🔍 HubConnection State: {state}");
            HubConnectionState = $"SignalR State: {state}";
        }
    }
}
