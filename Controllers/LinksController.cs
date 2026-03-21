using Microsoft.AspNetCore.Mvc;
using TheNestAPI.Data;
using TheNestAPI.Models;
using TheNestAPI.Adapters;

namespace TheNestAPI.Controllers
{
    [ApiController]
    [Route("links")]
    public class LinksController : ControllerBase
    {
        public LinksController(ApplicationDbContext context)
        {
            DbAdapter.Initialize(context);
            LinksAdapter.Initialize(context);
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<Link>> GetLink(string code)
        {
            try
            {
                Link link = await LinksAdapter.GetLink(code);
                return Ok(link);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult<string>> SaveLink([FromBody] string url)
        {
            if (!await DbAdapter.IsAuthorized(Request))
            {
                return Unauthorized("Invalid auth token.");
            }

            try
            {
                string code = await LinksAdapter.SaveLink(url);
                return Ok(code);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}