
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;


namespace prid_2021_g06.Models
{

    public class Board : IValidatableObject
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "3 caractères minimum, 25 caractères maximum.")]
        public string Name { get; set; }
        [Required]
        public virtual User Owner { get; set; }
        public virtual ICollection<BoardList> BoardLists { get; set; } = new HashSet<BoardList>();
        public virtual ICollection<UserBoard> UsersBoardsRelation { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var currContext = validationContext.GetService(typeof(g06Context)) as g06Context;
            Debug.Assert(currContext != null);

            if (!uniqueName(currContext))
                yield return new ValidationResult("Le nom est déjà utilisé...", new[] { nameof(Name) });
        }

        private bool uniqueName(g06Context context)
        {
            var b = context.Boards.SingleOrDefault(b => b.Name == Name && b.Owner.Pseudo == Owner.Pseudo);
            return b == null;
        }
        
        public override string ToString()
        {
            return "Id : "+ Id + ", nom : " + Name + ". Ce tableau détient " + BoardLists.Count + " liste(s) et " + UsersBoardsRelation.Count + " relations avec des utilisateurs. Le créateur est : {" + Owner.ToString() + "}";
        }
    }
}