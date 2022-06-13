namespace prid_2021_g06.Models
{
    public class BoardDTO : DTO
    {
        public string Name { get; set; }
        public UserDTO Owner { get; set; }
        public override string ClassType { get; set; } = "Board";

    }
}