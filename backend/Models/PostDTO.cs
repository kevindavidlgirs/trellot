namespace prid_2021_g06.Models
{
    public class PostDTO : DTO
    {
        public string Text { get; set; }
        public string PicturePath { get; set; }
        public virtual CardDTO Card { get; set; }
        public virtual UserDTO Owner { get; set; }
        public override string ClassType { get; set; } = "Post";
    }
}