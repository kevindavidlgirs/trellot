using System.ComponentModel.DataAnnotations;
namespace prid_2021_g06.Models {

    public class Tag {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual Card Card { get; set; }
    }
}