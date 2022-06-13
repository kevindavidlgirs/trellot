using System;
using System.Collections.Generic;


namespace prid_2021_g06.Models
{
    public class UserDTO : DTO
    {
        public string Pseudo { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime? BirthDate { get; set; }
        public Role Role { get; set; }
        public string PicturePath { get; set; }
        public IList<PhoneDTO> Phones { get; set; }
        public override string ClassType { get; set; } = "User";
    }
}