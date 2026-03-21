using Microsoft.AspNetCore.Mvc;
using TheNestAPI.Data;
using TheNestAPI.Models;
using TheNestAPI.Models.Twitch;
using TheNestAPI.Domain;
using TheNestAPI.Adapters;

namespace TheNestAPI.Controllers
{
    [ApiController]
    [Route("notes")]
    public class NotesController : ControllerBase
    {
        public NotesController(ApplicationDbContext context)
        {
            DbAdapter.Initialize(context);
            NotesAdapter.Initialize(context);
            LinksAdapter.Initialize(context);
        }

        [HttpGet]
        public async Task<ActionResult<List<Note>>> GetNotes()
        {
            if (!await DbAdapter.IsAuthorized(Request))
            {
                return Unauthorized("Invalid auth token.");
            }

            return await NotesAdapter.GetNotes();
        }

        [HttpPut("clip")]
        public async Task<ActionResult<string>> CreateClip([FromBody] BotNote data)
        {
            if (!await DbAdapter.IsAuthorized(Request))
            {
                return Unauthorized("Invalid auth token.");
            }

            Console.WriteLine($"Creating clip for note: {data.Description} by {data.Username}");
            try
            {
                StreamInfo streamInfo = await TwitchAdapter.GetStreamInfo("plopparntv");
                VodInfo vodInfo = await TwitchAdapter.GetLatestVodInfo(streamInfo.UserId);
                
                string? clipUri = await TwitchAdapter.CreateClip("475237486");
                Console.WriteLine($"Clip URI: {clipUri}");
                if (clipUri == null)
                {
                    return BadRequest("Failed to create clip.");
                }

                string shortenedClipUri = await LinksAdapter.SaveLink(clipUri);
                Console.WriteLine($"Shortened Clip URI: {shortenedClipUri}");

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

                await NotesAdapter.SaveNote(note);
                return $"https://plopparn.tv/clips/{shortenedClipUri}";
            } catch (Exception)
            {
                return BadRequest($"Failed to create note.");
            }

        }

        [HttpPost("{id}/used")]
        public async Task<ActionResult<List<Note>>> ToggleUsedStatus(int id)
        {
            if (!await DbAdapter.IsAuthorized(Request))
            {
                return Unauthorized("Invalid auth token.");
            }

            if (await NotesAdapter.ToggleNoteUsed(id))
            {
                return await NotesAdapter.GetNotes();
            }

            return NotFound($"No note with id \"{id}\" found.");
        }

        [HttpPost("{id}/processed")]
        public async Task<ActionResult<List<Note>>> ToggleProcessedStatus(int id)
        {
            if (!await DbAdapter.IsAuthorized(Request))
            {
                return Unauthorized("Invalid auth token.");
            }

            if (await NotesAdapter.ToggleNoteProcessed(id))
            {
                return await NotesAdapter.GetNotes();
            }

            return NotFound($"No note with id \"{id}\" found.");
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult<List<Note>>> ToggleDeletedStatus(int id)
        {
            if (!await DbAdapter.IsAuthorized(Request))
            {
                return Unauthorized("Invalid auth token.");
            }

            if (await NotesAdapter.ToggleNoteDeleted(id))
            {
                return await NotesAdapter.GetNotes();
            }

            return NotFound($"No note with id \"{id}\" found.");
        }

        [HttpGet("{user}")]
        public async Task<ActionResult<int>> GetUserNoteUsedCount(string user)
        {
            return await NotesAdapter.GetNotesUsedCount(user);
        }
    }
}
