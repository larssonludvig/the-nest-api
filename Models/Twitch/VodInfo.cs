using System.Text.Json;

namespace TheNestAPI.Models.Twitch
{
    public class VodInfo
    {
        public string Id { get; set; } = "";
        public string StreamId { get; set; } = "";
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";

        public static VodInfo ToDomain(JsonElement json)
        {
            return new VodInfo
            {
                Id = json.GetProperty("id").GetString() ?? "",
                StreamId = json.GetProperty("stream_id").GetString() ?? "",
                UserId = json.GetProperty("user_id").GetString() ?? "",
                UserName = json.GetProperty("user_name").GetString() ?? "",
                Title = json.GetProperty("title").GetString() ?? "",
                Url = json.GetProperty("url").GetString() ?? ""
            };
        }
    }
}

