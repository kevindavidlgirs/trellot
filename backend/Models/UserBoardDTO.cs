namespace prid_2021_g06.Models
{
    public class UserBoardDTO : DTO
    {
        public int BoardId { get; set; }
        public UserDTO User { get; set; }
        public bool IsOwner { get; set; }
        public bool UserIsInvitedOnTheBoard { get; set; }
        public override string ClassType { get; set; } = "UserBoard";
    }
}