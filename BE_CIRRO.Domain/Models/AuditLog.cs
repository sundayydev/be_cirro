using BE_CIRRO.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Domain.Models
{
    public class AuditLog : ModelBase
    {
        [Key]
        public Guid LogId { get; set; }
        

        public Guid UserId { get; set; }
        

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required]
        public string Action { get; set; } 

        [Required]
        public string TargetType { get; set; } 

        [Required]
        public Guid TargetId { get; set; }
        
    }
}
