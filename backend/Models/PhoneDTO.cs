namespace prid_2021_g06.Models
{
    public class PhoneDTO : DTO
    {
        public int PhoneId { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public override string ClassType { get; set; } = "Phone";
    }
}