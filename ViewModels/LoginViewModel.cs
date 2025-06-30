using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Hoplings.DriverApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string message;

        public IAsyncRelayCommand LoginCommand { get; }

        private readonly HttpClient _httpClient;

        public LoginViewModel()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://dispatch.hoplings.com")
            };

            LoginCommand = new AsyncRelayCommand(LoginAsync);
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                Message = "⚠️ Please enter your email and password.";
                return;
            }

            var loginRequest = new
            {
                email = Email,
                password = Password
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginRequest);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);

                    var token = doc.RootElement.GetProperty("token").GetString();

                    if (!string.IsNullOrEmpty(token))
                    {
                        await SecureStorage.Default.SetAsync("jwt_token", token);
                        Message = "✅ Login successful!";

                        // ✅ Navigate to MainPage
                        await Shell.Current.GoToAsync("//MainPage");
                    }
                    else
                    {
                        Message = "⚠️ Login succeeded but no token returned.";
                    }
                }
                else
                {
                    Message = "❌ Invalid email or password.";
                }
            }
            catch (Exception ex)
            {
                Message = $"🚫 Error: {ex.Message}";
            }
        }
    }
}
