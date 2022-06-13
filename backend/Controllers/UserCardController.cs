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
    public class UserCardsController : Controller
    {
        public UserCardsController(g06Context context, IHubContext<GeneralHub, IGeneralHubService> hubContext) : base(context, hubContext){}


        [HttpGet("{cardid}")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUserscardRelation(int cardid)
        {
            // Vérifie que c'est soit l'owner du "board" ou un utilisateur invité qui cherche à récupérer les relations 
            var UsersCardsRelationList = await _context.UsersCardsRelation.Include("User").Where(uc => uc.CardId == cardid &&
            (uc.Card.BoardList.Board.Owner.Pseudo == User.Identity.Name 
            || uc.Card.BoardList.Board.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name))).ToListAsync();
            
            if (UsersCardsRelationList == null) { return NotFound(); }
            var userCards = new List<User>();
            foreach (var ucr in UsersCardsRelationList)
            {
                userCards.Add(ucr.User);
            }
            return userCards.ToDTO();
        }

        [HttpPost]
        public async Task<ActionResult<UserCardDTO>> addUserCardRelation(UserCardDTO usercard)
        {
            // Vérifie que l'utilisateur connecté est soit l'owner du "board" ou un utilisateur invité sur le "board"
            var ub = await _context.Cards.FirstOrDefaultAsync(c => c.Id == usercard.Card.Id  &&  (c.BoardList.Board.Owner.Pseudo == User.Identity.Name 
                                                                                                || c.BoardList.Board.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name)));
            if (ub == null){ return Unauthorized(); }
            var card = await _context.Cards.FindAsync(usercard.Card.Id);
            var user = await _context.Users.FindAsync(usercard.User.Id);
            if (card == null || user == null) { return NotFound(); }

            // On bloque l'ajout de l'owner sur sa propre "card"
            if(card.Owner.Pseudo == user.Pseudo) { return Unauthorized(); }

            // Recherche si l'utilisateur trouvé précédemment à bien une relation avec le "board"
            var userBoard = await _context.UsersBoardsRelation.FirstOrDefaultAsync(ub => ub.Board.BoardLists.Any(bl => bl.Cards.Any(c => c == card)) && (ub.User == user || ub.Board.Owner == user));
            if (userBoard == null)
            {
                // Si aucune relation "userboard" n'existe nous recherchons à savoir si celui qu'on ajout à la "card" est l'owner de la "board"
                // Puisque l'owner ne se trouve pas dans les relation avec sa "board"
                var owner = await _context.Boards.FirstOrDefaultAsync(b => b.BoardLists.Any(bl => bl.Cards.Any(c => c == card)) && b.Owner == user);
                if(owner == null)
                    return Unauthorized();
            }
            var userCard = new UserCard { Card = card, User = user };
            _context.UsersCardsRelation.Add(userCard);
            await _context.SaveChangesAsync();
            try
            {
                await _hubContext.Clients.All.Notify(userCard.ToDTO());
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
            return Ok();
        }

        [HttpDelete("{cardId}/{userId}")]
        public async Task<ActionResult<UserCardDTO>> removeUserCardRelation(int cardId, int userId)
        {
            var ub = await _context.Cards.FirstOrDefaultAsync(c => c.Id == cardId  &&  (c.BoardList.Board.Owner.Pseudo == User.Identity.Name 
                                                                                                || c.BoardList.Board.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name)));
            if (ub == null){ return Unauthorized(); }
            var card = await _context.Cards.FindAsync(cardId);
            var user = await _context.Users.FindAsync(userId);
            if (card == null || user == null) { return NotFound(); }
            var userCard = await _context.UsersCardsRelation.FirstOrDefaultAsync(uc => uc.Card == card && uc.User == user);
            if (userCard == null) { return NotFound(); }
            var elementDeleted = userCard.ToDTO();
            elementDeleted.Deleted = true;
            _context.UsersCardsRelation.Remove(userCard);
            await _context.SaveChangesAsync();
            try
            {
                await _hubContext.Clients.All.Notify(elementDeleted);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
            return Ok();
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