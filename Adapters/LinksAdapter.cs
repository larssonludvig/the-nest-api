using Microsoft.EntityFrameworkCore;
using TheNestAPI.Data;
using TheNestAPI.Models;

namespace TheNestAPI.Adapters
{
    public static class LinksAdapter
    {
        private static ApplicationDbContext _context;

        public static void Initialize(ApplicationDbContext context)
        {
            _context = context;
        }

        public static async Task<Link> GetLink(string key)
        {
            Link? link = await _context.Links
                .Where(x =>
                    x.Code == key
                ).FirstOrDefaultAsync();
            return link ?? throw new Exception("Link not found");
        }

        public static async Task<string> SaveLink(string url)
        {
            string code = Guid.NewGuid().ToString("N")[..10];
            Link link = new()
            {
                Code = code,
                Url = url
            };

            try
            {
                _context.Links.Add(link);
                await DbAdapter.SaveChanges();
                return code;
            } catch (Exception ex)
            {
                Console.WriteLine($"Error saving link: {ex.Message}");
                throw ex;
            }
        }
    }
}