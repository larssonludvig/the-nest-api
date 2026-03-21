using Microsoft.EntityFrameworkCore;
using TheNestAPI.Data;
using TheNestAPI.Models;

namespace TheNestAPI.Adapters
{
    public static class NotesAdapter
    {
        private static ApplicationDbContext _context;

        public static void Initialize(ApplicationDbContext context)
        {
            _context = context;
        }

        public static async Task<List<Note>> GetNotes()
        {
            return await _context?.Notes
                .Where(x =>
                    !x.Deleted &&
                    (
                        DateTime.Compare(DateTime.Now.AddDays(-14), x.Created ?? DateTime.Now.AddDays(-15)) <= 0 ||
                        x.ClipURI != null
                    )
                ).ToListAsync();
        }

        public static async Task<Note> GetNote(int id)
        {
            return await _context.Notes
                .Where(x =>
                    x.Id == id
                ).FirstOrDefaultAsync();
        }

        public static async Task SaveNote(Note note)
        {
            _context.Notes.Add(note);
            await DbAdapter.SaveChanges();
        }

        public static async Task<int> GetNotesUsedCount(string user)
        {
            return await _context.Notes
                .Where(x =>
                    x.Username.ToLower() == user.ToLower() &&
                    x.Used == true &&
                    !x.Deleted
                 ).CountAsync();
        }

        public static async Task<bool> ToggleNoteUsed(int id) 
        {
            Note note = await GetNote(id);
            if (note == null)
                return false;

            note.Used = !note.Used;
            await DbAdapter.SaveChanges();
            return true;
        }

        public static async Task<bool> ToggleNoteProcessed(int id)
        {
            Note note = await GetNote(id);
            if (note == null)
                return false;

            note.Processed = !note.Processed;
            await DbAdapter.SaveChanges();
            return true;
        }

        public static async Task<bool> ToggleNoteDeleted(int id)
        {
            Note note = await GetNote(id);
            if (note == null)
                return false;

            note.Deleted = !note.Deleted;
            await DbAdapter.SaveChanges();
            return true;
        }
    }
}