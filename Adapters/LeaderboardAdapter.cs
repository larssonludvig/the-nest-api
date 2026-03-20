using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheNestAPI.Data;
using TheNestAPI.Models;
using TheNestAPI.Models.Twitch;
using TheNestAPI.Domain;
using TheNestAPI.Adapters;
using System.Text.Json;

namespace TheNestAPI.Adapters
{
    public static class LeaderboardAdapter
    {
        private static ApplicationDbContext _context;

        private static readonly string SEASON = "s9";

        public static void Initialize(ApplicationDbContext context)
        {
            _context = context;
        }

        public static async Task<JsonElement> GetLeaderboard()
        {
            return await GetLeaderboard("");
        }

        public static async Task<JsonElement> GetLeaderboard(string mode)
        {
            HttpClient client = new();

            HttpResponseMessage response = await client.GetAsync($"https://api.the-finals-leaderboard.com/v1/leaderboard/{SEASON}{mode}/crossplay");

            string content = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(content);
            JsonElement data = json.RootElement.GetProperty("data");

            return data;
        }
    }
}