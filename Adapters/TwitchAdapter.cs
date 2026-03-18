using System.Net.Http.Headers;
using System.Text.Json;

namespace TheNestAPI.Adapters
{
    public static class TwitchAdapter
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static async Task<bool> IsStreamerLive(string user)
        {
            HttpClient client = await SetupHttpClient();
            HttpResponseMessage response = await client.GetAsync($"https://api.twitch.tv/helix/streams?user_login={user}");
            string content = await response.Content.ReadAsStringAsync();

            JsonDocument json = JsonDocument.Parse(content);
            JsonElement data = json.RootElement.GetProperty("data");

            return data.GetArrayLength() > 0;
        }

        public static async Task<string?> CreateClip(string broadcasterId)
        {
            HttpClient client = await SetupHttpClient();
            HttpResponseMessage response = await client.PostAsync($"https://api.twitch.tv/helix/clips?broadcaster_id={broadcasterId}", null);
            string content = await response.Content.ReadAsStringAsync();

            JsonDocument json = JsonDocument.Parse(content);
            JsonElement data = json.RootElement.GetProperty("data");

            if (data.GetArrayLength() > 0)
            {
                return data[0].GetProperty("id").GetString();
            }

            return null;
        }

        private static async Task<HttpClient> SetupHttpClient()
        {
            HttpClient client = new();
            string clientId = Environment.GetEnvironmentVariable("Twitch_ClientId") ?? "";
            string token = await RefreshAccessToken() ?? "";

            client.DefaultRequestHeaders.Add("Client-Id", clientId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        public static async Task<string?> RefreshAccessToken()
        {
            HttpClient client = new();
            string clientId = Environment.GetEnvironmentVariable("Twitch_ClientId") ?? "";
            string clientSecret = Environment.GetEnvironmentVariable("Twitch_ClientSecret") ?? "";
            string refreshToken = Environment.GetEnvironmentVariable("Twitch_RefreshToken") ?? "";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(refreshToken))
            {
                throw new InvalidOperationException("Twitch env credentials are not properly configured.");
            }

            var requestBody = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "client_credentials" },
                { "refresh_token", refreshToken }
            };

            var response = await client.PostAsync("https://id.twitch.tv/oauth2/token", new FormUrlEncodedContent(requestBody));
            var content = await response.Content.ReadAsStringAsync();

            var json = JsonDocument.Parse(content);
            return json.RootElement.GetProperty("access_token").GetString();
        }
    }
}