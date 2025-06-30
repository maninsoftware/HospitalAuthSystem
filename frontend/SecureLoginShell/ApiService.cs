using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace HospitalLoginApp.Services
{
    public static class ApiService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string baseUrl = "http://127.0.0.1:8000";

        public static async Task<bool> VerifyCredentials(string username, string password)
        {
            var values = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            };
            var content = new FormUrlEncodedContent(values);

            var response = await httpClient.PostAsync($"{baseUrl}/login/", content);
            return response.IsSuccessStatusCode;
        }

        public static async Task<string?> VerifyFace(byte[] imageBytes)
        {
            using var httpClient = new HttpClient();

            using var content = new MultipartFormDataContent();
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(imageContent, "file", "face.jpg");

            var response = await httpClient.PostAsync($"{baseUrl}/verify/", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("username", out var prop))  // ✅ updated key
                {
                    return prop.GetString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[JSON Parse Error] " + ex.Message);
            }

            return null;
        }

        public static async Task<string?> RegisterUser(string username, string password, byte[] imageBytes)
        {
            using var httpClient = new HttpClient();

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(username), "username");
            content.Add(new StringContent(password), "password");

            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(imageContent, "file", "face.jpg");

            var response = await httpClient.PostAsync("http://127.0.0.1:8000/register/", content);
            if (response.IsSuccessStatusCode)
            {
                return "✅ Registration successful!";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                return $"❌ Registration failed: {err}";
            }
        }

    }
}
