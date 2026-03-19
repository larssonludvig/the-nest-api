using Microsoft.AspNetCore.Mvc;
using TheNestAPI.Adapters;

namespace TheNestAPI.Controllers
{
    [ApiController]
    [Route("twitch")]
    public class TwitchController : ControllerBase
    {
        [HttpGet("live")]
        public async Task<ActionResult<bool>> getLiveStatus()
        {
            return await TwitchAdapter.IsStreamerLive("plopparntv");
        }
    }
}