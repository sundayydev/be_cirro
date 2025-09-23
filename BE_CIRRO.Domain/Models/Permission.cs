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
    public class Permission : ModelBase
    {
        [Key]
        public Guid PermissionId { get; set; }
        

        public Guid UserId { get; set; }
        

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public Guid? FileId { get; set; }
        

        [ForeignKey(nameof(FileId))]
        public File File { get; set; }

        public Guid? FolderId { get; set; }
        

        [ForeignKey(nameof(FolderId))]
        public Folder Folder { get; set; }

        [Required]
        [MaxLength(20)]
        public string PermissionType { get; set; } 
    }
}
