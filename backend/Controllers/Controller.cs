using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using prid_2021_g06.Models;
using Microsoft.AspNetCore.SignalR;
using prid_2021_g06.Service;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;


namespace prid_tuto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class Controller : ControllerBase
    {
        protected readonly g06Context _context;
        protected IHubContext<GeneralHub, IGeneralHubService> _hubContext;

        public Controller(g06Context context, IHubContext<GeneralHub, IGeneralHubService> hubContext = null)
        {
            _context = context;
            _hubContext = hubContext;
        }
        
        protected void deletePictureForPost(Post p)
        {
            if (p.PicturePath != null)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", p.PicturePath);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }

        [HttpPost("upload")]
        public abstract Task<IActionResult> Upload([FromForm] string pseudo, [FromForm] IFormFile picture);

        [HttpPost("cancel")]
        public abstract IActionResult Cancel([FromBody] dynamic data);

        [HttpPost("confirm")]
        public abstract IActionResult Confirm([FromBody] dynamic data);
    }
}