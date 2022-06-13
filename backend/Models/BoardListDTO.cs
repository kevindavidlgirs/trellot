using System.Collections.Generic;

namespace prid_2021_g06.Models
{
    public class BoardListDTO : DTO
    {
        public string Name { get; set; }
        public int BoardId { get; set; }
        public List<CardDTO> Cards { get; set; }
        public override string ClassType { get; set; } = "BoardList";
    }
}