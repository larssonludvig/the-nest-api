using Microsoft.AspNetCore.Mvc;
using TheNestAPI.Adapters;

namespace TheNestAPI.Controllers
{
    [ApiController]
    [Route("twitch")]
    public class TwitchController : ControllerBase
    {
        public TwitchController(IConfiguration configuration)
        {
            TwitchAdapter.Initialize(configuration);
        }

        [HttpGet("live")]
        public async Task<ActionResult<bool>> getLiveStatus()
        {
            return await TwitchAdapter.IsStreamerLive("plopparntv");
        }
    }



}