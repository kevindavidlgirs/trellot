using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace prid_2021_g06.Models
{
    public class UserBoard
    {
        public int BoardId { get; set; }
        [Required]
        public virtual Board Board { get; set; }
        public int UserId { get; set; }
        [Required]
        public virtual User User { get; set; }

        [NotMapped]
        public bool IsOwner { get; set; }

        [NotMapped]
        public bool UserIsInvitedOnTheBoard { get; set; }
        public override string ToString()
        {
            return "Relation entre le tableau : {" + Board.ToString() + "} et l'utilisateur : {" + User.ToString() + "}";
        }

    }
}