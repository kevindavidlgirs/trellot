using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prid_2021_g06.Models;
using PRID_Framework;
using System.Linq;
using prid_2021_g06.Service;
using Microsoft.AspNetCore.SignalR;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace prid_tuto.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BoardListsController : Controller
    {

        public BoardListsController(g06Context context, IHubContext<GeneralHub, IGeneralHubService> hubContext) : base(context, hubContext) { }

        [HttpGet("{boardId}")]
        public async Task<ActionResult<IEnumerable<BoardListDTO>>> GetByBoardId(int boardId)
        {
            // Récupère un "boardList" si l'owner du "board" est égal à l'utilisateur connecté ou si l'utilisateur connecté est en relation avec ce "board"
            var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == boardId && b.Owner.Pseudo == User.Identity.Name
                                                            || b.Id == boardId && b.UsersBoardsRelation.Any(bl => bl.User.Pseudo == User.Identity.Name));
            if (board == null) { return NotFound(); }

            var boardList = await _context.BoardLists.Include("Cards").Where(bl => bl.Board == board).ToListAsync();
            foreach (var bl in boardList)
            {
                bl.Cards = bl.Cards.OrderBy(c => c.indexIntoBoardList).ToList();
            }
            // le foreach suivant a pour but de récupérer les users liés à chaque carte
            foreach (var e in boardList)
            {
                foreach (var i in e.Cards)
                {
                    var p = await _context.UsersCardsRelation.Where(uc => uc.CardId == i.Id).ToListAsync();
                    foreach (var m in p)
                    {
                        i.Users.Add(_context.Users.FirstOrDefault(u => u.Id == m.UserId));
                    }
                }
            }
            return boardList.ToDTO();
        }

        [HttpPost("{boardId}")]
        public async Task<ActionResult<BoardListDTO>> AddBoardList(int boardId, BoardListDTO blList)
        {
            // Récupère un "boardList" si l'owner du "board" est égal à l'utilisateur connecté ou si l'utilisateur connecté est en relation avec ce "board"
            var board = await _context.Boards.Include("BoardLists").FirstOrDefaultAsync(b => b.Id == boardId && b.Owner.Pseudo == User.Identity.Name
                                                                || b.Id == boardId && b.UsersBoardsRelation.Any(b => b.User.Pseudo == User.Identity.Name));
            if (board == null) { return NotFound(); }
            if (board.BoardLists.Count() > 7) { return Unauthorized("La limite des 'boardLists' est atteinte. Seul 8 sont autorisées."); }

            var newBoardList = new BoardList()
            {
                Name = blList.Name
            };
            board.BoardLists.Add(newBoardList);
            var res = await _context.SaveChangesAsyncWithValidation();
            if (!res.IsEmpty) { return BadRequest(res); }

            // WebSocket
            // Permet de notifier d'un changement tous les clients connectés au HUB 
            try
            {
                await _hubContext.Clients.All.Notify(newBoardList.ToDTO());
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

            return newBoardList.ToDTO();
            //return CreatedAtAction(nameof(GetOneBoard), new { name = b.Name }, newBoard);
        }

        [HttpPut("{newName}/{boardId}")]
        public async Task<IActionResult> updateBoardListName(string newName, int boardId, BoardListDTO blDTO)
        {
            // Récupère un "boardList" égal au "boardList.id" reçu 
            // On vérifie que le "board" lié au "boardList" est égal au "boardId" reçu
            // On vérifie que l'owner du "board" est lié à l'utilisateur connecté
            // ou que l'utilisateur connecté est invité sur le "board"
            var bl = await _context.BoardLists.Include("Board").FirstOrDefaultAsync(bl => bl.Id == blDTO.Id && bl.Board.Id == boardId
                                                                                && bl.Board.Owner.Pseudo == User.Identity.Name
                                                                                || bl.Id == blDTO.Id && bl.Board.Id == boardId
                                                                                && bl.Board.UsersBoardsRelation.Any(bl => bl.User.Pseudo == User.Identity.Name));
            if (bl == null) { return NotFound(); }
            bl.Name = newName;

            var res = await _context.SaveChangesAsyncWithValidation();
            if (!res.IsEmpty) { return BadRequest(res); }

            // WebSocket
            // Permet de notifier d'un changement tous les clients connectés au HUB
            try
            {
                await _hubContext.Clients.All.Notify(bl.ToDTO());
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

            return NoContent();
        }

        [HttpDelete("{boardListId}")]
        public async Task<ActionResult<Boolean>> DeleteBoardList(int boardListId)
        {
            // Récupère un "boardList" égal au "boardList.id" reçu 
            // On vérifie que l'owner du "board" est lié à l'utilisateur connecté
            // ou que l'utilisateur connecté est invité sur le "board"
            var bl = await _context.BoardLists.SingleOrDefaultAsync(bl => bl.Id == boardListId && bl.Board.Owner.Pseudo == User.Identity.Name
                                        || bl.Id == boardListId && bl.Board.UsersBoardsRelation.Any(bl => bl.User.Pseudo == User.Identity.Name));
            if (bl == null) { return NotFound(); }
            var cards = await _context.Cards.Where(c => c.BoardList == bl).ToListAsync();
            foreach (var card in cards)
            {
                var posts = await _context.Posts.Where(p => p.Card.Id == card.Id).ToListAsync();
                foreach (var p in posts)
                {
                    this.deletePictureForPost(p);
                    _context.Posts.Remove(p);
                }
                var tags = await _context.Tags.Where(t => t.Card == card).ToListAsync();
                foreach (var t in tags)
                {
                    _context.Tags.Remove(t);
                }
                _context.Cards.Remove(card);
            }

            var elementDeleted = bl.ToDTO();
            _context.BoardLists.Remove(bl);
            await _context.SaveChangesAsync();

            // WebSocket
            // Permet de notifier d'un changement tous les clients connectés au HUB
            try
            {
                await _hubContext.Clients.All.Notify(elementDeleted);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

            return true;
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

