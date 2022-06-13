using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prid_2021_g06.Models;
using PRID_Framework;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using prid_2021_g06.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace prid_tuto.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : Controller
    {
        public CardsController(g06Context context, IHubContext<GeneralHub, IGeneralHubService> hubContext) : base(context, hubContext) { }

        [HttpGet("{cardId}")]
        public async Task<ActionResult<CardDTO>> GetCard(int cardId)
        {
            // Récupère une "card" égale au "cardId" reçu 
            // On vérifie que l'owner du "board" est lié à l'utilisateur connecté
            // ou que l'utilisateur connecté est invité sur le "board"
            var card = await _context.Cards.Include("Owner").FirstOrDefaultAsync(c => c.Id == cardId && c.BoardList.Board.Owner.Pseudo == User.Identity.Name
                                                || c.Id == cardId && c.BoardList.Board.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name));
            if (card == null) { return NotFound(); }

            return card.ToDTO();
        }

        [HttpPost("{boadListId}")]
        public async Task<ActionResult<CardDTO>> AddCard(int boadListId, CardDTO cardDTO)
        {
            // Récupère un "boardList" égale au "boardListId" reçu 
            // On vérifie que l'owner du "board" est lié à l'utilisateur connecté
            // ou que l'utilisateur connecté est invité sur le "board"
            var bl = await _context.BoardLists.Include("Cards").FirstOrDefaultAsync(bl => bl.Id == boadListId && bl.Board.Owner.Pseudo == User.Identity.Name
                                                                                        || bl.Id == boadListId && bl.Board.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name));
            if (bl == null) { return NotFound(); }
            // if (bl == null || _context.Boards.All(b => b.BoardLists.Sum(bl1 => bl1.Cards.Count()) > 9)) {
            //     return Unauthorized(); 
            // }

            // Le code ci-dessus fonctionne pas avec Sql Server, mais bien avec MySql.
            // En cause, la présence de sum().
            // Pour y remedier, le code ci-dessous.

            // Limitation du nombre de "cards"
            var nbrLimite = 0;
            var board = _context.Boards.Include("BoardLists").FirstOrDefault(b => b.BoardLists.Any(bl => bl.Id == boadListId));
            var bls = _context.BoardLists.Include("Cards").Where(bl => bl.Board == board);
            foreach (var i in bls)
            {
                nbrLimite += i.Cards.Count();
            }

            if (nbrLimite > 20) { return Unauthorized("La limite des 'cards' est atteinte. Seul 20 sont autorisées."); }

            var newCard = new Card() { Name = cardDTO.Name, indexIntoBoardList = bl.Cards.Count(), Owner = _context.Users.FirstOrDefault(u => u.Pseudo == User.Identity.Name) };
            bl.Cards.Add(newCard);
            var res = await _context.SaveChangesAsyncWithValidation();
            if (!res.IsEmpty) { return BadRequest(res); }

            // WebSocket
            // Permet de notifier d'un changement tous les clients connectés au HUB
            try
            {
                await _hubContext.Clients.All.Notify(newCard.ToDTO());
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

            return newCard.ToDTO();
        }

        [HttpPut("update")]
        public async Task<IActionResult> PutCard(CardDTO cardDTO)
        {
            // Récupère une "card" égale au "cardDTO.id" reçu 
            // On vérifie que l'owner du "board" est lié à l'utilisateur connecté
            // ou que l'utilisateur connecté est invité sur le "board"
            var card = await _context.Cards.FirstOrDefaultAsync(c => c.Id == cardDTO.Id && c.BoardList.Board.Owner.Pseudo == User.Identity.Name
                                                                    || c.Id == cardDTO.Id && c.BoardList.Board.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name));
            if (card == null) { return NotFound(); }

            card.Name = cardDTO.Name;

            var res = await _context.SaveChangesAsyncWithValidation();
            if (!res.IsEmpty) { return BadRequest(res); }

            // WebSocket
            // Permet de notifier d'un changement tous les clients connectés au HUB
            try
            {
                await _hubContext.Clients.All.Notify(card.ToDTO());
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

            return NoContent();
        }

        [HttpPut("{boardListId}/{newIndexPosition}")]
        public async Task<IActionResult> PutCardPosition(int boardListId, int newIndexPosition, CardDTO cardDTO)
        {
            // Récupère une "card" égale au "cardDTO.id" reçu 
            // On vérifie que l'owner du "board" est lié à l'utilisateur connecté
            // ou que l'utilisateur connecté est invité sur le "board"
            var card = await _context.Cards.Include("BoardList").FirstOrDefaultAsync(
                c => c.Id == cardDTO.Id && c.BoardList.Board.Owner.Pseudo == User.Identity.Name
                 || c.Id == cardDTO.Id && c.BoardList.Board.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name));

            if (card == null) { return NotFound(); }
            var boardList = await _context.BoardLists.Include("Cards").FirstOrDefaultAsync(bl => bl.Id == boardListId);

            if (card.BoardList != boardList)
            {
                // Change l'indexe d'une "card" lorsqu'on l'envoie vers une autre "boardList"
                card.changeIndexAfterMovingIntoOtherBoardList(_context, boardList, newIndexPosition);
            }
            else
            {
                // Change l'indexe d'une "card" lorsqu'on la bouge dans la même "boardList"
                card.changeIndexAfterMovingIntoSameBoardList(boardList, newIndexPosition);
            }
            await _context.SaveChangesAsync();

            // WebSocket
            // Permet de notifier d'un changement tous les clients connectés au HUB
            try
            {
                await _hubContext.Clients.All.Notify(card.ToDTO());
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

            return NoContent();
        }

        [HttpDelete("{cardId}")]
        public async Task<ActionResult> DeleteCard(int cardId)
        {
            // Récupère une "card" égale au "cardId" reçu 
            // On vérifie que l'owner du "board" est lié à l'utilisateur connecté
            // ou que l'utilisateur connecté est invité sur le "board"
            var card = await _context.Cards.Include("BoardList").FirstOrDefaultAsync(c => c.Id == cardId && c.BoardList.Board.Owner.Pseudo == User.Identity.Name
                                            || c.Id == cardId && c.BoardList.Board.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name));
            if (card == null) { return NotFound(); }

            var posts = await _context.Posts.Where(p => p.Card == card).ToListAsync();
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
            await _context.SaveChangesAsync();
            var boardList = await _context.BoardLists.Include("Cards").FirstOrDefaultAsync(bl => bl == card.BoardList);

            card.changeOtherIndexBeforeDelete(boardList);
            var elementDeleted = card.ToDTO();
            _context.Cards.Remove(card);
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
