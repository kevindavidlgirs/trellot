using System.Collections.Generic;

namespace prid_2021_g06.Models
{
    public class CardDTO : DTO
    {
        public string Name { get; set; }
        public int boardListId { get; set; }
        public UserDTO Owner { get; set; }
        public override string ClassType { get; set; } = "Card";
        public List<UserDTO> Users {get; set;}
        public List<TagDTO> Tags {get; set;}
    }
}