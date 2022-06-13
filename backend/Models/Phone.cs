using System.ComponentModel.DataAnnotations;

namespace prid_2021_g06.Models
{
    public class Phone
    {

        [Key]
        public int Id { get; set; }

        public int PhoneId { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Number { get; set; }

        [Required]
        public virtual User User { get; set; }

        public override string ToString()
        {
            return "Id : " + Id + ", phoneId : " + PhoneId + ", type : " + Type + ", number : " + Number + ". Ce numéro de téléphone est lié à l'utilisateur : {"+ User.ToString() + "}";
        }
    }
}