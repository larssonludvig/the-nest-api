using System.Net.Http.Headers;
using System.Text.Json;
using TheNestAPI.Models;

namespace TheNestAPI.Adapters
{
    public static class TwitchAdapter
    {
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
                return data[0].GetProperty("edit_url").GetString();
            }

            return null;
        }

        public static async Task<StreamInfo> GetStreamInfo(string user)
        {
            HttpClient client = await SetupHttpClient();

            HttpResponseMessage response = await client.GetAsync($"https://api.twitch.tv/helix/streams?user_login={user}");
            if (response.IsSuccessStatusCode)
            {
                string context = await response.Content.ReadAsStringAsync();
                JsonDocument json = JsonDocument.Parse(context);
                JsonElement data = json.RootElement.GetProperty("data");

                if (data.GetArrayLength() > 0)
                {
                    return StreamInfo.ToDomain(data[0]);
                }
            }

            throw new Exception($"Failed to get stream info for user {user}. {response}");
        }

        public static async Task<VodInfo> GetLatestVodInfo(string userId)
        {
            HttpClient client = await SetupHttpClient();

            HttpResponseMessage response = await client.GetAsync($"https://api.twitch.tv/helix/videos?user_id={userId}");
            if (response.IsSuccessStatusCode)
            {
                string context = await response.Content.ReadAsStringAsync();
                JsonDocument json = JsonDocument.Parse(context);
                JsonElement data = json.RootElement.GetProperty("data");

                if (data.GetArrayLength() > 0)
                {
                    return VodInfo.ToDomain(data[0]);
                }
            }

            throw new Exception($"Failed to get vod info for user {userId}. {response}");
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

        private static async Task<string?> RefreshAccessToken()
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
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            var response = await client.PostAsync("https://id.twitch.tv/oauth2/token", new FormUrlEncodedContent(requestBody));
            var content = await response.Content.ReadAsStringAsync();

            var json = JsonDocument.Parse(content);
            return json.RootElement.GetProperty("access_token").GetString();
        }
    }
}