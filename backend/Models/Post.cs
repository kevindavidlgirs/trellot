using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;


namespace prid_2021_g06.Models
{

    public class Post
    {
        [Key]
        public int Id { get; set; }
        [StringLength(1000, MinimumLength = 2, ErrorMessage = "2 caractères minimum, 1000 caractères maximum.")]
        public string Text { get; set; }
        public string PicturePath { get; set; }
        [Required]
        public virtual Card Card { get; set; }
        [Required]
        public virtual User Owner { get; set; }
        
        public override string ToString()
        {
            return "Id : " + Id + ", texte : " + Text + ". Ce message est lié à la carte : {" + Card.ToString() + "}";
        }
    }
}