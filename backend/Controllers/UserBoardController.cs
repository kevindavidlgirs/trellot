using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prid_2021_g06.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using prid_2021_g06.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;

namespace prid_tuto.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserBoardsController : Controller
    {
        public UserBoardsController(g06Context context, IHubContext<GeneralHub, IGeneralHubService> hubContext) : base(context, hubContext){}

        [HttpGet("{boardid}")]
        public async Task<ActionResult<IEnumerable<UserBoardDTO>>> GetUsersBoardRelation(int boardId)
        {
            // Vérifie si l'utilisateur connecté est l'owner du "board" ou si il est invité sur le "board"
            var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == boardId && (b.Owner.Pseudo == User.Identity.Name
                                                                    || b.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name)));
            if (board == null) { return NotFound(); }
            var UsersBoardsRelationList = await _context.UsersBoardsRelation.Where(pb => pb.BoardId == boardId).ToListAsync();
            var allUsers = await _context.Users.ToListAsync();
            var userOwner = await _context.Users.Where(u => u.Boards.Any(b => b.Id == boardId)).SingleOrDefaultAsync();
            allUsers.Remove(userOwner);
            var usersLinkedToBoard = new List<UserBoard>();
            usersLinkedToBoard.Add(new UserBoard() { BoardId = boardId, User = userOwner, UserIsInvitedOnTheBoard = false, IsOwner = true });
            foreach (var user in allUsers)
            {
                usersLinkedToBoard.Add(new UserBoard() { BoardId = boardId, User = user, UserIsInvitedOnTheBoard = UsersBoardsRelationList.Any(ub => ub.UserId == user.Id), IsOwner = false });
            }
            return usersLinkedToBoard.ToDTO();
        }

        [HttpPost]
        public async Task<ActionResult<UserBoardDTO>> addUserBoardRelation(UserBoardDTO ubRelation)
        {
            var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == ubRelation.BoardId && b.Owner.Pseudo == User.Identity.Name);
            if (board == null) return NotFound();
            
            // Nous bloquons l'ajout de l'owner sur la board
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == ubRelation.User.Id && u.Pseudo != User.Identity.Name);
            if (user == null) return Unauthorized();

            var ub = await _context.UsersBoardsRelation.FirstOrDefaultAsync(ub => ub.User == user && ub.Board == board);
            if (ub != null) return NotFound();
            var userBoard = new UserBoard { Board = board, User = user };
            _context.UsersBoardsRelation.Add(userBoard);
            
            // WebSocket
            // Permet de notifier d'un changement tous les clients connectés au HUB
            try
            {
                await _hubContext.Clients.All.Notify(userBoard.ToDTO());
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{boardId}/{userId}")]
        public async Task<IActionResult> RemoveUserBoardRelation(int boardId, int userId)
        {
            // Vérifie que l'utilisateur connecté est l'owner du "board"
            // ensuite on recherche la bonne relation à supprimer grâce aux "ID" reçus 
            var userBoardRelation = await _context.UsersBoardsRelation.FirstOrDefaultAsync(ubr => ubr.Board.Owner.Pseudo == User.Identity.Name 
                                                                                            && ubr.Board.Id == boardId && ubr.User.Id == userId);
            if (userBoardRelation == null) { return NotFound(); }
            var cards = await _context.Cards.Where(c => c.BoardList.Board.Id == boardId).ToListAsync();

            // On supprimer d'abord les relations existantes avec l'utilisateur sur toutes les "card" du "board".
            foreach(var c in cards){
                var uc = await _context.UsersCardsRelation.FirstOrDefaultAsync(uc => uc.Card == c && uc.User.Id == userId);
                if(uc != null) { _context.UsersCardsRelation.Remove(uc); }
            } 
            await _context.SaveChangesAsync();
            var userBoard = userBoardRelation.ToDTO();
            userBoard.Deleted = true;
            _context.UsersBoardsRelation.Remove(userBoardRelation);

            // WebSocket
            // Permet de notifier d'un changement tous les clients connectés au HUB
            try
            {
                userBoard.Deleted = true;
                await _hubContext.Clients.All.Notify(userBoard);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
            await _context.SaveChangesAsync();
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