namespace prid_2021_g06.Models
{
    public abstract class DTO
    {
        public int Id { get; set; }
        public abstract string ClassType { get; set; }
        public bool Deleted { get; set; }
    }
}