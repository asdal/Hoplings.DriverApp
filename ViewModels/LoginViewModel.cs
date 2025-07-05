using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Storage;
using System.Diagnostics; // ✅ Needed for Debug.WriteLine

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

            Debug.WriteLine("✅ HttpClient initialized with BaseAddress: " + _httpClient.BaseAddress);

            LoginCommand = new AsyncRelayCommand(LoginAsync);
            Debug.WriteLine("✅ LoginCommand initialized.");
        }

        private async Task LoginAsync()
        {
            Debug.WriteLine("➡️ LoginAsync started.");
            Debug.WriteLine($"📧 Email: {Email}");
            Debug.WriteLine($"🔑 Password: {(string.IsNullOrEmpty(Password) ? "EMPTY" : "SET")}");

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                Message = "⚠️ Please enter your email and password.";
                Debug.WriteLine("⚠️ Validation failed: Missing email or password.");
                return;
            }

            var loginRequest = new
            {
                email = Email,
                password = Password
            };

            try
            {
                Debug.WriteLine("📡 Sending login request to /api/Auth/login ...");

                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginRequest);

                Debug.WriteLine($"✅ HTTP Response Status: {(int)response.StatusCode} {response.ReasonPhrase}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("📄 Received response JSON: " + json);

                    var doc = JsonDocument.Parse(json);

                    var token = doc.RootElement.GetProperty("token").GetString();

                    if (!string.IsNullOrEmpty(token))
                    {
                        Debug.WriteLine("🔑 Token received: " + token);

                        await SecureStorage.Default.SetAsync("jwt_token", token);
                        Debug.WriteLine("🔒 Token saved to SecureStorage with key 'jwt_token'.");

                        Message = "✅ Login successful!";
                        Debug.WriteLine("🚀 Navigating to //MainPage ...");
                        await Shell.Current.GoToAsync("//MainPage");
                    }
                    else
                    {
                        Message = "⚠️ Login succeeded but no token returned.";
                        Debug.WriteLine("⚠️ No token found in response JSON.");
                    }
                }
                else
                {
                    Message = "❌ Invalid email or password.";
                    Debug.WriteLine("❌ Login failed with status code: " + (int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Message = $"🚫 Error: {ex.Message}";
                Debug.WriteLine("🚫 Exception during login: " + ex);
            }
        }
    }
}
