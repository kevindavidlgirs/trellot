using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace prid_2021_g06.Models
{

    public class BoardList : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "3 caractères minimum, 15 caractères maximum.")]
        public string Name { get; set; }
        [Required]
        public virtual Board Board { get; set; }
        public virtual ICollection<Card> Cards { get; set; } = new HashSet<Card>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var currContext = validationContext.GetService(typeof(g06Context)) as g06Context;
            Debug.Assert(currContext != null);

            if (!uniqueName(currContext))
                yield return new ValidationResult("Le nom est déjà utilisé...", new[] { nameof(Name) });
        }

        private bool uniqueName(g06Context context)
        {
            var bl = context.BoardLists.SingleOrDefault(bl => bl.Name == Name && bl.Board == Board);
            return bl == null;
        }

        public override string ToString()
        {
            return "Id : " + Id + ", nom : " + Name + ". Cette liste détient " + Cards.Count + " cartes et est liée au tableau : {" + Board.ToString() + "}";
        }
    }
}