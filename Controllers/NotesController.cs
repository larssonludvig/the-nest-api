using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheNestAPI.Data;
using TheNestAPI.Models;
using TheNestAPI.Domain;
using TheNestAPI.Adapters;

namespace TheNestAPI.Controllers
{
    [ApiController]
    [Route("notes")]
    public class NotesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public NotesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<List<Note>>> GetNotes()
        {
            string authToken = Request.Headers["Authorization"];
            string storedToken = await _context.Generic
                .Where(x => x.Key == "notesAuth")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            if (storedToken == null || authToken != storedToken)
            {
                return Unauthorized("Invalid auth token.");
            }

            return await GetNonDeletedNotes();
        }

        [HttpPut("clip")]
        public async Task<ActionResult<string>> CreateClip([FromBody] BotNote data)
        {
            string authToken = Request.Headers["Authorization"];
            string storedToken = await _context.Generic
                .Where(x => x.Key == "notesAuth")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            if (storedToken == null || authToken != storedToken)
            {
                return Unauthorized("Invalid auth token.");
            }

            StreamInfo streamInfo = await TwitchAdapter.GetStreamInfo("plopparntv");
            VodInfo vodInfo = await TwitchAdapter.GetLatestVodInfo(streamInfo.UserId);
            string? clipUri = await TwitchAdapter.CreateClip("475237486");

            if (clipUri == null)
            {
                return BadRequest("Failed to create clip.");
            }

            TimeSpan elapsed = DateTime.Now - streamInfo.StartTime;
            Note note = new()
            {
                Description = data.Description,
                Username = data.Username,
                ClipURI = clipUri,
                Game = streamInfo.GameName,
                ElapsedTime = $"{elapsed.Hours:D2}h{elapsed.Minutes:D2}m{elapsed.Seconds:D2}s",
                Created = DateTime.Now,
                StreamId = vodInfo.StreamId,
                offset = 0
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return clipUri;
        }

        [HttpPost("{id}/used")]
        public async Task<ActionResult<List<Note>>> ToggleUsedStatus(int id)
        {
            string authToken = Request.Headers["Authorization"];
            string storedToken = await _context.Generic
                .Where(x => x.Key == "notesAuth")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            if (storedToken == null || authToken != storedToken)
            {
                return Unauthorized("Invalid auth token.");
            }

            var entity = await _context.Notes
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (entity != null)
            {
                entity.Used = !entity.Used;
                await _context.SaveChangesAsync();
                return await _context.Notes
                    .Where(x => x.Id == id)
                    .ToListAsync();
            }

            return NotFound($"No note with id \"{id}\" found.");
        }

        [HttpPost("{id}/processed")]
        public async Task<ActionResult<List<Note>>> ToggleProcessedStatus(int id)
        {
            string authToken = Request.Headers["Authorization"];
            string storedToken = await _context.Generic
                .Where(x => x.Key == "notesAuth")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            if (storedToken == null || authToken != storedToken)
            {
                return Unauthorized("Invalid auth token.");
            }

            var entity = await _context.Notes
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (entity != null)
            {
                entity.Processed = !entity.Processed;
                await _context.SaveChangesAsync();
                return await _context.Notes
                    .Where(x => x.Id == id)
                    .ToListAsync();
            }

            return NotFound($"No note with id \"{id}\" found.");
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult<List<Note>>> ToggleDeletedStatus(int id)
        {
            string authToken = Request.Headers["Authorization"];
            string storedToken = await _context.Generic
                .Where(x => x.Key == "notesAuth")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            if (storedToken == null || authToken != storedToken)
            {
                return Unauthorized("Invalid auth token.");
            }

            var entity = await _context.Notes
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (entity != null)
            {
                entity.Deleted = !entity.Deleted;
                await _context.SaveChangesAsync();
                return await GetNonDeletedNotes();
            }

            return NotFound($"No note with id \"{id}\" found.");
        }

        [HttpGet("{user}")]
        public async Task<ActionResult<int>> GetUserNoteUsedCount(string user)
        {
            return await _context.Notes
                .Where(
                    x => x.Username.ToLower() == user.ToLower() &&
                    x.Used == true &&
                    x.Deleted == false
                ).CountAsync();
        }

        private async Task<List<Note>> GetNonDeletedNotes()
        {
            return await _context.Notes
                .Where(x =>
                    x.Deleted == false &&
                    (DateTime.Compare(DateTime.Now.AddDays(-14), x.Created ?? DateTime.Now.AddDays(-15)) <= 0 || x.ClipURI != null)
                )
                .ToListAsync();
        }

        private bool LessThanTwoWeeks(DateTime date)
        {
            return DateTime.Compare(DateTime.Now.AddDays(-14), date) <= 0;
        }
    }
}
