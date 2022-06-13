using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prid_2021_g06.Models;
using PRID_Framework;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using prid_2021_g06.Service;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace prid_tuto.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BoardsController : Controller
    {
        public BoardsController(g06Context context, IHubContext<GeneralHub, IGeneralHubService> hubContext) : base(context, hubContext) { }


        [HttpGet("{boardId}")]
        public async Task<ActionResult<BoardDTO>> GetBoard(int boardId)
        {
            // Récupère un "board" si l'owner est égal à l'utilisateur connecté ou si l'utilisateur connecté est en relation avec ce "board"
            var Board = await _context.Boards.Include("Owner").FirstOrDefaultAsync(b => b.Id == boardId && b.Owner.Pseudo == User.Identity.Name
                                                                                    || b.Id == boardId && b.UsersBoardsRelation.Any(bl => bl.User.Pseudo == User.Identity.Name));
            if (Board == null) { return NotFound(); }
            return Board.ToDTO();
        }

        [HttpGet("getallboardsfromauser")]
        public async Task<ActionResult<IEnumerable<BoardDTO>>> GetAllBoardsFromOneUser()
        {
            return (await _context.Boards.Include("Owner").Where(b => b.Owner.Pseudo == User.Identity.Name).ToListAsync()).ToDTO();
        }

        [HttpGet("getallboardswhereuserisinvited")]
        public async Task<ActionResult<IEnumerable<BoardDTO>>> GetAllBoardsWhereUserIsInvited()
        {
            return (await _context.Boards.Include("Owner").Where(b => b.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name)).ToListAsync()).ToDTO();
        }

        [HttpPost]
        public async Task<ActionResult<BoardDTO>> AddBoard(BoardDTO boardDTO)
        {
            var user = await _context.Users.Include("Boards").FirstOrDefaultAsync(u => u.Pseudo == User.Identity.Name);
            if (user == null) { return NotFound(); }
            if (user.Boards.Count() > 9) { return Unauthorized("La limite des 'boards' est atteinte. Seul 10 sont autorisées."); }
            var newBoard = new Board() { Name = boardDTO.Name };
            user.Boards.Add(newBoard);
            var res = await _context.SaveChangesAsyncWithValidation();
            if (!res.IsEmpty) { return BadRequest(res); }
            
            // WebSocket
            // Permet de notifier d'un changement tous les clients connectés au HUB
            try
            {
                await _hubContext.Clients.All.Notify(newBoard.ToDTO());
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
            return Ok();
        }

        [HttpPut("{name}")]
        public async Task<IActionResult> updateBoardName(string name, BoardDTO boardDTO)
        {
            var board = await _context.Boards.Include("Owner").FirstOrDefaultAsync(b => b.Id == boardDTO.Id && b.Owner.Pseudo == User.Identity.Name);
            if (board == null) { return NotFound(); }
            board.Name = name;
            var res = await _context.SaveChangesAsyncWithValidation();
            if (!res.IsEmpty) { return BadRequest(res); }

            // WebSocket
            // Permet de notifier d'un changement tous les clients connectés au HUB
            try
            {
                await _hubContext.Clients.All.Notify(board.ToDTO());
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteBoards([FromQuery(Name = "boardId")] List<int> boardsId)
        {
            BoardDTO elementDeleted = null;

            foreach (int bId in boardsId)
            {
                var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == bId && b.Owner.Pseudo == User.Identity.Name);
                if (board == null) { return Unauthorized(); }

                var blList = await _context.BoardLists.Where(bl => bl.Board.Id == board.Id).ToListAsync();
                foreach (var bl in blList)
                {
                    var cards = await _context.Cards.Where(c => c.BoardList == bl).ToListAsync();
                    foreach (var card in cards)
                    {
                        var posts = await _context.Posts.Where(p => p.Card == card).ToListAsync();
                        foreach (var p in posts)
                        {
                            this.deletePictureForPost(p);
                            _context.Posts.Remove(p);
                        }
                        var tags = await _context.Tags.Where(t => t.Card == card).ToListAsync();
                        foreach(var t in tags){
                            _context.Tags.Remove(t);
                        }
                        _context.Cards.Remove(card);
                    }
                    _context.BoardLists.Remove(bl);
                }
                elementDeleted = board.ToDTO();
                _context.Boards.Remove(board);
                await _context.SaveChangesAsync();
            }
            // WebSocket
            // Permet de notifier d'un changement tous les clients connectés au HUB
            // On ne notifie que si l'élément est supprimé
            try
            {
                if (elementDeleted != null)
                {
                    elementDeleted.Deleted = true;
                    await _hubContext.Clients.All.Notify(elementDeleted);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

            return NoContent();
        }

        // Les méthodes suivantes n'ont pas d'utilité dans le contexte de ce contrôleur.
        public override Task<IActionResult> Upload([FromForm] string pseudo, [FromForm] IFormFile picture)
        {
            throw new NotImplementedException();
        }

        public override IActionResult Cancel([FromBody] dynamic data)
        {
            throw new NotImplementedException();
        }

        public override IActionResult Confirm([FromBody] dynamic data)
        {
            throw new NotImplementedException();
        }
    }
}