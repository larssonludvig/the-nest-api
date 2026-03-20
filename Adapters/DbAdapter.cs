using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheNestAPI.Data;
using TheNestAPI.Models;
using TheNestAPI.Models.Twitch;
using TheNestAPI.Domain;
using TheNestAPI.Adapters;

namespace TheNestAPI.Adapters
{
    public static class DbAdapter
    {
        private static ApplicationDbContext _context;

        public static void Initialize(ApplicationDbContext context)
        {
            _context = context;
        }

        public static async Task<bool> IsAuthorized(string authToken)
        {
            string storedToken = await _context.Generic
                .Where(x => x.Key == "notesAuth")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            return !(storedToken == null || authToken != storedToken);
        }
    }
}