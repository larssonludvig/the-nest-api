using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheNestAPI.Adapters;
using TheNestAPI.Data;

namespace TheNestAPI.Controllers
{
    [ApiController]
    [Route("user")]
    public class GenericController : ControllerBase
    {
        public GenericController(ApplicationDbContext context)
        {
            DbAdapter.Initialize(context);
            NotesAdapter.Initialize(context);
        }
    }
}
