namespace prid_2021_g06.Models
{
    public class UserCardDTO : DTO
    {
        public CardDTO Card { get; set; }
        public UserDTO User { get; set; }
        public override string ClassType { get; set; } = "UserCard";
    }
}