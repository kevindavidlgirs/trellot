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
using Microsoft.AspNetCore.Authorization;

namespace prid_tuto.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : Controller
    {
        public PostsController(g06Context context) : base(context) { }

        [HttpGet("{cardId}")]
        public async Task<ActionResult<IEnumerable<PostDTO>>> GetPost(int cardId)
        {
            // Pour récupérer les posts l'utilisateur doit être invité sur la "board"
            var posts = await _context.Posts.Include("Card").Where(p => p.Card.Id == cardId &&
                                                                    (p.Card.BoardList.Board.UsersBoardsRelation.Any(ub => ub.User.Pseudo == User.Identity.Name)
                                                                    || p.Card.BoardList.Board.Owner.Pseudo == User.Identity.Name)).ToListAsync();
            if (posts == null) { return NotFound(); }
            return posts.ToDTO();
        }

        [HttpPost]
        public async Task<ActionResult<PostDTO>> AddPost(PostDTO post)
        {
            // Pour ajouter des posts l'utilisateur doit être ajouté à la "card" ou être l'owner de la "card"
            var card = await _context.Cards.FirstOrDefaultAsync(c => c.Id == post.Card.Id
                                                                && (c.UsersCardsRelation.Any(uc => uc.User.Pseudo == User.Identity.Name)
                                                                || c.Owner.Pseudo == User.Identity.Name));
            if (card == null) { return NotFound(); }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Pseudo == User.Identity.Name);
            var newPost = new Post() { Text = post.Text, PicturePath = post.PicturePath, Owner = user };
            card.Posts.Add(newPost);
            var res = await _context.SaveChangesAsyncWithValidation();
            if (res != null) { return BadRequest(res); }
            return newPost.ToDTO();
        }

        [HttpDelete("{postId}")]
        public async Task<ActionResult> DeletePost(int postId)
        {
            // Pour supprimer des posts l'utilisateur doit être ajouté à la "cards" ou être l'owner de la "board"
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId
                                                                && (p.Card.UsersCardsRelation.Any(uc => uc.User.Pseudo == User.Identity.Name)
                                                                || p.Card.BoardList.Board.Owner.Pseudo == User.Identity.Name));
            if (post == null) { return NotFound(); }
            this.deletePictureForPost(post);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        public override async Task<IActionResult> Upload([FromForm] string pseudo, [FromForm] IFormFile picture)
        {
            if (picture != null && picture.Length > 0)
            {
                //var fileName = Path.GetFileName(picture.FileName);
                var fileName = pseudo + "-" + DateTime.Now.ToString("yyyyMMddHHmmssff") + ".jpg";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await picture.CopyToAsync(fileSrteam);
                }
                return Ok($"\"uploads/{fileName}\"");
            }
            return Ok();
        }

        // Les méthodes suivantes n'ont pas d'utilité dans le contexte de ce contrôleur.
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
