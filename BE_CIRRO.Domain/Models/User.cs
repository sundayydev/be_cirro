using BE_CIRRO.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Domain.Models
{

    public class User : ModelBase
    {
        [Key]
        public Guid UserId { set; get; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public string Role { get; set; } 
    }
}
