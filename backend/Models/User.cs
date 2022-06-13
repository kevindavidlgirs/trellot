using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;


namespace prid_2021_g06.Models
{
    public enum Role
    {
        Admin = 2, Manager = 1, User = 0
    }

    public class User : IValidatableObject
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "3 caractères minimum, 10 caractères maximum.")]
        [RegularExpression("^[a-zA-Z]([a-zA-Z0-9_])+", ErrorMessage = "Ne doit contenir que des lettres, nombres, ou underscore.")]
        public string Pseudo { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "3 caractères minimum, 10 caractères maximum.")]
        public string Password { get; set; }
        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "L'adresse email n'a pas un format valide !")]
        public string Email { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "3 caractères minimum, 50 caractères maximum.")]
        public string LastName { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "3 caractères minimum, 50 caractères maximum.")]
        public string FirstName { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? Age
        {
            get
            {
                if (!BirthDate.HasValue)
                    return null;
                var age = DateTime.Today.Year - BirthDate.Value.Year;
                if (BirthDate.Value.Date > DateTime.Today.AddYears(-age)) { --age; }
                return age;
            }
        }
        public string PicturePath { get; set; }
        public Role Role { get; set; } = Role.User;
        [NotMapped]
        public string Token { get; set; }
        public virtual ICollection<Board> Boards { get; set; } = new HashSet<Board>();
        public virtual ICollection<Card> Cards { get; set; } = new HashSet<Card>();
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
        public virtual IList<Phone> Phones { get; set; } = new List<Phone>();
        public virtual ICollection<UserBoard> UsersBoardsRelation { get; set; }
        public virtual ICollection<UserCard> UsersCardsRelation { get; set; }

        public string RefreshToken { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var currContext = validationContext.GetService(typeof(g06Context)) as g06Context;
            Debug.Assert(currContext != null);

            if (!uniquePseudo(currContext))
                yield return new ValidationResult("Le pseudo est déjà utilisé...", new[] { nameof(Pseudo) });
            if (!uniqueEmail(currContext))
                yield return new ValidationResult("Le email est déjà utilisé...", new[] { nameof(Email) });
            if (BirthDate.HasValue && BirthDate.Value.Date > DateTime.Today)
                yield return new ValidationResult("Nous ne pouvons pas voir le jour dans le futur !", new[] { nameof(BirthDate) });
            else if (Age.HasValue && Age < 18)
                yield return new ValidationResult("Il est préférable d'avoir plus de 18 ans.", new[] { nameof(BirthDate) });
            if (string.IsNullOrEmpty(LastName) && !string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName) && string.IsNullOrEmpty(FirstName))
            {
                yield return new ValidationResult("Si vous désidez d'ajouter un prénom le nom ne doit pas être vide et inversement aussi !", new[] { nameof(FirstName) });
            }
        }

        private bool uniquePseudo(g06Context context)
        {
            var u = context.Users.SingleOrDefault(u => u.Pseudo == Pseudo && u.Id != Id);
            return u == null;
        }

        private bool uniqueEmail(g06Context context)
        {
            var u = context.Users.SingleOrDefault(u => u.Email == Email && u.Id != Id);
            return u == null;
        }

        public override string ToString()
        {
            return "Id : " + Id + ", pseudo : " + Pseudo + ", e-mail : " + Email + ", prénom : " + FirstName + ", nom de famille : " + LastName + ", rôle : " + Role + ". Détient " + Boards.Count + " tableau(x), " + Cards.Count + " carte(s), " + Posts.Count + " post(s) et " + Phones.Count + " numéro(s) de téléphone.";
        }
    }
}
