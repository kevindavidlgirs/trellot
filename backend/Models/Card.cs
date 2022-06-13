using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;


namespace prid_2021_g06.Models
{

    public class Card : IValidatableObject
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "3 caractères minimum, 20 caractères maximum.")]
        public string Name { get; set; }
        [Required]
        public virtual User Owner { get; set; }
        [NotMapped]
        public IList<User> Users {get; set;} = new List<User>();
        public virtual ICollection<Tag> Tags {get; set;} = new HashSet<Tag>();
        public virtual BoardList BoardList { get; set; }
        [Required]
        public int indexIntoBoardList { get; set; }
        public virtual ICollection<Post> Posts {get; set;}
        public virtual ICollection<UserCard> UsersCardsRelation { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var currContext = validationContext.GetService(typeof(g06Context)) as g06Context;
            Debug.Assert(currContext != null);

            if (!uniqueName(currContext))
                yield return new ValidationResult("Le nom est déjà utilisé...", new[] { nameof(Name) });
        }

        private bool uniqueName(g06Context context)
        {
            var b = context.Boards.Where(b => b.BoardLists.Any(bl => bl == BoardList) && b.BoardLists.Any(bl => bl.Cards.Any(c => c.Name == Name && c.Id != Id))).SingleOrDefault();
            return b == null;
        }

        public void changeIndexAfterMovingIntoOtherBoardList(g06Context context, BoardList boardList, int newIndexPosition)
        {
            int lastPosition = this.indexIntoBoardList;
            this.indexIntoBoardList = newIndexPosition;
            var cardsList = context.Cards.Where(c => c.BoardList == this.BoardList).ToList();

            foreach (var c in cardsList)
            {
                if (c.indexIntoBoardList > lastPosition && c != this)
                {
                    c.indexIntoBoardList -= 1;
                }
            }

            this.BoardList = boardList;

            foreach (var c in boardList.Cards)
            {
                if (c.indexIntoBoardList >= this.indexIntoBoardList)
                    c.indexIntoBoardList += 1;
            }
        }

        public void changeIndexAfterMovingIntoSameBoardList(BoardList boardList, int newIndexPosition)
        {
            int lastPosition = this.indexIntoBoardList;
            this.indexIntoBoardList = newIndexPosition;
            this.BoardList = boardList;

            if (lastPosition < newIndexPosition)
            {
                foreach (var c in this.BoardList.Cards)
                {
                    if (c.indexIntoBoardList > lastPosition && c.indexIntoBoardList <= newIndexPosition && c != this)
                    {
                        c.indexIntoBoardList -= 1;
                    }
                }
            }
            else
            {
                foreach (var c in this.BoardList.Cards)
                {
                    if (c.indexIntoBoardList >= newIndexPosition && c.indexIntoBoardList < lastPosition && c != this)
                    {
                        c.indexIntoBoardList += 1;
                    }
                }
            }
        }

        public void changeOtherIndexBeforeDelete(BoardList boardList)
        {
            int lastPosition = this.indexIntoBoardList;

            foreach (var c in boardList.Cards)
            {
                if (c.indexIntoBoardList > lastPosition)
                {
                    c.indexIntoBoardList -= 1;
                }
            }
        }

        public override string ToString()
        {
            return "Id : " + Id + ", nom : " + Name + ". Cette carte détient " + Posts.Count + " posts et " + UsersCardsRelation.Count + " relation(s) avec des utilisateurs. Le créateur est : {" + Owner.ToString() + "}. Cette carte porte l'index " + indexIntoBoardList + " dans la liste : {" + BoardList.ToString() + "}";
        }
    }
}