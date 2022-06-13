using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prid_2021_g06.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace prid_tuto.Controllers {
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase {
        private readonly g06Context _context;
        public TagsController(g06Context context) {
            _context = context;
        }

        [HttpGet("{cardId}")]
        public async Task<ActionResult<IEnumerable<TagDTO>>> GetCard(int cardId) {
            // Vérifie que l'utilisateur connecté est l'owner de le "board" ou qu'il est invité sur le "board"
            var tags = await _context.Tags.Where(t => t.Card.Id == cardId && (t.Card.BoardList.Board.Owner.Pseudo == User.Identity.Name 
                                                                                ||  t.Card.BoardList.Board.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name))).ToListAsync();
            if (tags == null){
                return NotFound();
            }
            return tags.ToDTO();
        }

        [HttpPost("{cardId}")]
        public async Task<ActionResult<TagDTO>> AddPost(int cardId, Tag tag) {
            // Vérifie que l'utilisateur connecté est l'owner de la "card" ou qu'il est invité sur la "card"
            var card = await _context.Cards.FirstOrDefaultAsync(c => c.Id  == cardId && (c.Owner.Pseudo == User.Identity.Name 
                                                                                ||  c.UsersCardsRelation.Any(uc => uc.User.Pseudo == User.Identity.Name)));
            if (card == null){
                return NotFound();
            }
            if (card.Tags.Count() >= 3){
                return Unauthorized();
            }
            var newTag = new Tag() { Name = tag.Name, Card = card };
            card.Tags.Add(newTag);
            await _context.SaveChangesAsync();
            return newTag.ToDTO();
        }

        [HttpDelete("{tagId}")]
        public async Task<ActionResult> DeleteCard(int tagId) {
            // Vérifie que l'utilisateur connecté est l'owner de la "card" ou qu'il est invité sur la "card"
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagId && (t.Card.Owner.Pseudo == User.Identity.Name
                                                || t.Card.UsersCardsRelation.Any(uc => uc.User.Pseudo == User.Identity.Name)));
            if (tag == null) {
                return NotFound();
            }
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
