using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HospitalLoginApp.Models;

namespace HospitalLoginApp.Services
{
    public class AuthService
    {
        private readonly HttpClient _client;
        private const string BASE_URL = "http://127.0.0.1:8000";

        public AuthService()
        {
            _client = new HttpClient();
        }

        public async Task<bool> LoginWithCredentials(string username, string password)
        {
            var loginData = new LoginRequest { username = username, password = password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{BASE_URL}/login_password", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginWithFace()
        {
            var response = await _client.PostAsync($"{BASE_URL}/verify_webcam", null);
            return response.IsSuccessStatusCode;
        }
    }
}
