using System.Text.Json;

namespace TheNestAPI.Models.Twitch
{
    public class StreamInfo
    {
        public DateTime StartTime { get; set; } = DateTime.MinValue;
        public string Id { get; set; } = "";
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string GameName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Status { get; set; } = "";
        public int ViewerCount { get; set; } = 0;

        public static StreamInfo ToDomain(JsonElement json)
        {
            return new StreamInfo
            {
                StartTime = json.GetProperty("started_at").GetDateTime(),
                Id = json.GetProperty("id").GetString() ?? "",
                UserId = json.GetProperty("user_id").GetString() ?? "",
                UserName = json.GetProperty("user_name").GetString() ?? "",
                GameName = json.GetProperty("game_name").GetString() ?? "",
                Title = json.GetProperty("title").GetString() ?? "",
                Status = json.GetProperty("type").GetString() ?? "",
                ViewerCount = json.GetProperty("viewer_count").GetInt32()
            };
        }
    }
}

