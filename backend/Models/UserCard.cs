using System.ComponentModel.DataAnnotations;


namespace prid_2021_g06.Models
{
    public class UserCard
    {
        public int CardId { get; set; }
        [Required]
        public virtual Card Card { get; set; }
        public int UserId { get; set; }
        [Required]
        public virtual User User { get; set; }
        public override string ToString()
        {
            return "Relation entre la carte : {" + Card.ToString() + "} et l'utilisateur : {" + User.ToString() + "}";
        }
    }
}