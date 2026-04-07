using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheNestAPI.Data;
using TheNestAPI.Models;
using TheNestAPI.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using TheNestAPI.Adapters;

namespace TheNestAPI.Controllers
{
    [ApiController]
    [Route("leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaderboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("{name}")]
        public async Task<ActionResult<LeaderboardEntry>> GetLeaderboardEntry(string name)
        {
            LeaderboardLastWeek? entry = await _context.LeaderboardLastWeek
                .Where(x => x.Name == name)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();

            Leagues league = await _context.Leagues
                .Where(x => x.Id == entry.LeagueNumber)
                .FirstOrDefaultAsync();

            if (entry == null)
                return NotFound("Failed to get leaderboard by username.");

            return new LeaderboardEntry
            {
                Name = entry.Name,
                Rank = entry.RankScore,
                League = league.Name,
                Timestamp = entry.Timestamp
            };
        }

        [HttpGet]
        [Route("{name}/history")]
        public async Task<ActionResult<LeaderboardHistory>> GetLeaderboardHistoryOfUser(string name, [FromQuery(Name = "from")] DateTime? from, [FromQuery(Name = "to")] DateTime? to)
        {
            List<LeaderboardLastWeek> res;

            if (from != null && to != null)
            {
                res = await _context.LeaderboardLastWeek
                    .Where(x =>
                        x.Name == name
                    ).ToListAsync();
            }
            else
            {
                res = await _context.LeaderboardLastWeek.Where(x =>
                    x.Name == name
                ).ToListAsync();
            }

            return new LeaderboardHistory
            {
                Name = name,
                Ranks = res.Select(x => x.RankScore).ToList(),
                Timestamps = res.Where(x => x.Timestamp.HasValue).Select(x => x.Timestamp.Value).ToList()
            };
        }

        [HttpGet]
        [Route("ruby/history")]
        public async Task<ActionResult<LeaderboardHistory>> GetRubyHistory([FromQuery(Name = "from")] DateTime? from, [FromQuery(Name = "to")] DateTime? to)
        {
            DateTime now = DateTime.Now;

            List<LeaderboardLastWeek> res;

            if (from != null && to != null)
            {
                res = await _context.LeaderboardLastWeek
                    .Where(x =>
                        x.RankPosition == 500
                    ).ToListAsync();
            }
            else
            {
                res = await _context.LeaderboardLastWeek.Where(x =>
                    x.RankPosition == 500
                ).ToListAsync();
            }

            return new LeaderboardHistory
            {
                Name = "Ruby",
                Ranks = res.Select(x => x.RankScore).ToList(),
                Timestamps = res.Where(x => x.Timestamp.HasValue).Select(x => x.Timestamp.Value).ToList()
            };
        }

        [HttpGet]
        [Route("{name}/history/all")]
        public async Task<ActionResult<UserAll>> GetAllFromUser(string name)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Name == name);

            if (user == null)
                return NotFound($"User {name} not found.");

            var result = await (from leaderboard in _context.LeaderboardS10
                                join league in _context.Leagues
                                on leaderboard.LeagueNumber equals league.Id
                                where leaderboard.Name == name
                                select new
                                {
                                    leaderboard.Name,
                                    leaderboard.RankPosition,
                                    leaderboard.ChangeAmount,
                                    leaderboard.LeagueNumber,
                                    leaderboard.RankScore,
                                    leaderboard.Timestamp,
                                    LeagueName = league.Name
                                }).ToListAsync();

            return new UserAll
            {
                Name = user.Name,
                XboxName = user.XboxName,
                PsnName = user.PsnName,
                SteamName = user.SteamName,
                ClubTag = user.ClubTag,
                Entries = result.Select(x => new LeaderboardEntry
                {
                    Name = x.Name,
                    Rank = x.RankPosition,
                    Change = x.ChangeAmount,
                    League = x.LeagueName,
                    Timestamp = x.Timestamp
                }).ToList()
            };
        }

        [HttpGet]
        [Route("{name}/string")]
        public async Task<string> getUserString(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "PlopparnTV";
            }

            JsonElement data = await LeaderboardAdapter.GetLeaderboard();

            foreach (var item in data.EnumerateArray())
            {
                if (item.GetProperty("name").GetString().Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    return $"{item.GetProperty("name")} is rank {item.GetProperty("rank")} with {item.GetProperty("rankScore")} RS";
                }
            }
            return $"{name} not found in top 10k";
        }

        [HttpGet]
        [Route("ruby/string")]
        public async Task<string> getUserString()
        {
            JsonElement data = await LeaderboardAdapter.GetLeaderboard();

            var last = data[500];

            return $"The border to Ruby is currently {last.GetProperty("rankScore")} RS";
        }

        [HttpGet]
        [Route("wt/{name}/string")]
        public async Task<string> getWTUserString(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "PlopparnTV";
            }

            JsonElement data = await LeaderboardAdapter.GetLeaderboard("worldtour");

            foreach (var item in data.EnumerateArray())
            {
                if (item.GetProperty("name").GetString().Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    return $"{item.GetProperty("name")} is rank {item.GetProperty("rank")} with {item.GetProperty("cashouts").GetInt64().ToString("N0")}$ in world tour";
                }
            }
            return $"{name} not found in top 10k";
        }

        [HttpGet]
        [Route("tdm/{name}/string")]
        public async Task<string> getTDMUserString(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "PlopparnTV";
            }

            JsonElement data = await LeaderboardAdapter.GetLeaderboard("teamdeathmatch");

            foreach (var item in data.EnumerateArray())
            {
                if (item.GetProperty("name").GetString().Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    return $"{item.GetProperty("name")} is rank {item.GetProperty("rank")} with {item.GetProperty("points").GetInt64().ToString("N0")}$ points";
                }
            }
            return $"{name} not found in top 10k";
        }


    }
}