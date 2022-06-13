using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prid_2021_g06.Models;
using PRID_Framework;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using prid_tuto.Helpers;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using prid_2021_g06.Service;
using Microsoft.AspNetCore.SignalR;

namespace prid_tuto.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly TokenHelper _tokenHelper;
        public UsersController(g06Context context, IHubContext<GeneralHub, IGeneralHubService> hubContext) : base(context, hubContext)
        {
            _tokenHelper = new TokenHelper(context);
        }

        [Authorized(Role.Admin)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            return (await _context.Users.ToListAsync()).ToDTO();
        }

        [AllowAnonymous]
        [HttpGet("{pseudo}")]
        public async Task<ActionResult<UserDTO>> GetOne(string pseudo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Pseudo == pseudo);
            if (user == null) { return NotFound(); }
            return user.ToDTO();
        }

        [AllowAnonymous]
        [HttpGet("email/{email}")]
        public async Task<ActionResult<UserDTO>> GetOneByEmail(string email)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound();
            return user.ToDTO();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(UserDTO userDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Pseudo == userDTO.Pseudo);
            if (user != null)
            {
                var err = new ValidationErrors().Add("Pseudo already in use", nameof(user.Pseudo)); //!
                return BadRequest(err);
            }
            if (userDTO.Email == null)
            {
                userDTO.Email = "default@default.be";
            }
            var newUser = new User()
            {
                Pseudo = userDTO.Pseudo,
                Password = userDTO.Password,
                Email = userDTO.Email,
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                BirthDate = userDTO.BirthDate,
                Role = userDTO.Role,
                PicturePath = userDTO.PicturePath
            };
            if (userDTO.Phones != null)
                foreach (var phone in userDTO.Phones.Where(p => p.PhoneId == 0))
                {
                    var p = new Phone
                    {
                        Type = phone.Type,
                        Number = phone.Number,
                        User = user
                    };
                    newUser.Phones.Add(p);
                    _context.Add(p);
                }
            _context.Users.Add(newUser);
            var res = await _context.SaveChangesAsyncWithValidation();
            if (!res.IsEmpty) { return BadRequest(res); }
            try
            {
                await _hubContext.Clients.All.Notify(newUser.ToDTO());
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
            return CreatedAtAction(nameof(GetOne), new { pseudo = userDTO.Pseudo }, newUser.ToDTO());
        }


        [Authorized(Role.Admin)]
        [HttpPut("{pseudo}")]
        public async Task<IActionResult> PutUser(string pseudo, UserDTO userDTO)
        {
            if (pseudo != userDTO.Pseudo) { return BadRequest(); }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Pseudo == pseudo);
            if (user == null) { return NotFound(); };
            if (userDTO.Password != null) { user.Password = userDTO.Password; }
            user.Pseudo = userDTO.Pseudo;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.BirthDate = userDTO.BirthDate;
            user.Role = userDTO.Role;

            /**  
            * Mise à jour des téléphones 
            */

            // récupère les id's non nuls des téléphones qui sont dans le DTO 
            var ids = userDTO.Phones.Where(p => p.PhoneId != 0).Select(p => p.PhoneId);
            // supprime de la DB les téléphones du membre dont les id's ne sont pas dans cette liste 
            foreach (var phone in user.Phones)
            {
                if (!ids.Contains(phone.PhoneId))
                {
                    _context.Remove(phone);
                }
            }
            // ajoute les nouveaux téléphones (ceux dont l'id est zéro) 
            foreach (var phone in userDTO.Phones.Where(p => p.PhoneId == 0))
            {
                var p = new Phone
                {
                    Type = phone.Type,
                    Number = phone.Number,
                    User = user
                };
                _context.Add(p);
            }
            if (!string.IsNullOrWhiteSpace(userDTO.PicturePath))
                user.PicturePath = userDTO.PicturePath + "?" + DateTime.Now.Ticks;
            else
                user.PicturePath = null;
            var res = await _context.SaveChangesAsyncWithValidation();
            if (!res.IsEmpty) { return BadRequest(res); }

            try
            {
                await _hubContext.Clients.All.Notify(user.ToDTO());
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

            return NoContent();
        }

        [HttpDelete("{pseudo}")]
        public async Task<IActionResult> DeleteUser(string pseudo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Pseudo == pseudo);
            if (user == null) { return NotFound(); }

            // On vérifie que que c'est soit l'utilisteur ou l'administrateur qui cherche à supprimer le profile
            if (user.Pseudo != User.Identity.Name)
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Pseudo == User.Identity.Name);
                if (u.Role != Role.Admin)
                {
                    return Unauthorized();
                }
            }

            // Ces conditions sont nécessaire pour laisser le temps à chaque méthode de supprimer en base de donnée
            if (this.deleteCreationsAndRelations(user).Result)
                if (this.deleteInvitedRelations(user).Result)
                    if (this.deleteOtherCreations(user).Result)
                        await this.deletePhones(user);

            var elementDeleted = user.ToDTO();
            _context.Users.Remove(user);
            elementDeleted.Deleted = true;
            await _context.SaveChangesAsync();

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

        private async Task<Boolean> deleteCreationsAndRelations(User user)
        {
            // Nous supprimons toutes les relations exitantes pour les "cards" et "boards" ansi que toutes les créations
            // Pour finir nous supprimons les "boards" appartenant à l'utilisateur
            var boards = await _context.Boards.Where(b => b.Owner.Pseudo == user.Pseudo).ToListAsync();
            foreach (var board in boards)
            {
                var blList = await _context.BoardLists.Where(bl => bl.Board.Id == board.Id).ToListAsync();
                foreach (var bl in blList)
                {
                    var cards = await _context.Cards.Where(c => c.BoardList == bl).ToListAsync();
                    foreach (var card in cards)
                    {
                        var ucList = await _context.UsersCardsRelation.Where(uc => uc.Card == card).ToListAsync();
                        foreach (var uc in ucList)
                        {
                            _context.UsersCardsRelation.Remove(uc);
                        }
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
                        _context.Cards.Remove(card);
                    }
                    _context.BoardLists.Remove(bl);
                    await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();
                _context.Boards.Remove(board);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        private async Task<Boolean> deleteInvitedRelations(User user)
        {
            // Nous supprimmons ici toutes les relations qu'a l'utilisateur sur d'autres "boards" et "cards" que les siennes
            // Donc sur les "boards" et "cards" sur lesquels il a été invité
            var userCardsRelationList = await _context.UsersCardsRelation.Where(uc => uc.User == user).ToListAsync();
            foreach (var uc in userCardsRelationList)
            {
                _context.UsersCardsRelation.Remove(uc);
            }
            await _context.SaveChangesAsync();


            var userBoardsRelationList = await _context.UsersBoardsRelation.Where(ub => ub.User == user).ToListAsync();
            foreach (var ub in userBoardsRelationList)
            {
                _context.UsersBoardsRelation.Remove(ub);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<Boolean> deleteOtherCreations(User user)
        {
            //Suppression de toutes les "cards" créées par l'utilisateur + suppression de ses posts
            var cardsCreateByUser = await _context.Cards.Where(c => c.Owner.Id == user.Id).ToListAsync();
            foreach (var c in cardsCreateByUser)
            {
                var posts = await _context.Posts.Where(p => p.Card == c).ToListAsync();
                foreach (var p in posts)
                {
                    this.deletePictureForPost(p);
                    _context.Posts.Remove(p);
                }
                await _context.SaveChangesAsync();
                var boardList = await _context.BoardLists.Include("Cards").FirstOrDefaultAsync(bl => bl == c.BoardList);
                c.changeOtherIndexBeforeDelete(boardList);
                _context.Cards.Remove(c);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        private async Task<Boolean> deletePhones(User user)
        {
            // Suppression des numéros de téléphones associés à l'utilisateur 
            var phones = await _context.Phones.Where(p => p.User == user).ToListAsync();
            foreach (var p in phones)
            {
                _context.Phones.Remove(p);
            }
            await _context.SaveChangesAsync();
            return true;
        }



        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<ActionResult<User>> Authenticate(UserDTO userDTO)
        {
            var user = await Authenticate(userDTO.Pseudo, userDTO.Password);

            if (user == null)
                return BadRequest(new ValidationErrors().Add("User not found", "Pseudo"));
            if (user.Token == null)
                return BadRequest(new ValidationErrors().Add("Incorrect password", "Password"));

            return Ok(user);
        }

        private async Task<User> Authenticate(string pseudo, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(p => p.Pseudo == pseudo);

            // return null if member not found
            if (user == null)
                return null;

            var hash = TokenHelper.GetPasswordHash(password);

            if (user.Password == password)
            {
                // authentication successful so generate jwt token
                user.Token = TokenHelper.GenerateJwtToken(user.Pseudo, user.Role);
                // Génère un refresh token et le stocke dans la table Members
                var refreshToken = TokenHelper.GenerateRefreshToken();
                await _tokenHelper.SaveRefreshTokenAsync(pseudo, refreshToken);
            }

            // remove password before returning
            user.Password = null;

            return user;
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

        public override IActionResult Cancel([FromBody] dynamic data)
        {
            string picturePath = data.picturePath;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", picturePath);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            return Ok();
        }

        public override IActionResult Confirm([FromBody] dynamic data)
        {
            string pseudo = data.pseudo;
            string picturePath = data.picturePath;
            string newPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", pseudo + ".jpg");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", picturePath);
            if (System.IO.File.Exists(path))
            {
                if (System.IO.File.Exists(newPath))
                    System.IO.File.Delete(newPath);
                System.IO.File.Move(path, newPath);
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<TokensDTO>> Refresh([FromBody] TokensDTO tokens)
        {
            var principal = TokenHelper.GetPrincipalFromExpiredToken(tokens.Token);
            var pseudo = principal.Identity.Name;
            var savedRefreshToken = await _tokenHelper.GetRefreshTokenAsync(pseudo);
            if (savedRefreshToken != tokens.RefreshToken)
                throw new SecurityTokenException("Invalid refresh token");

            var newToken = TokenHelper.GenerateJwtToken(principal.Claims);
            var newRefreshToken = TokenHelper.GenerateRefreshToken();
            await _tokenHelper.SaveRefreshTokenAsync(pseudo, newRefreshToken);

            return new TokensDTO
            {
                Token = newToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}