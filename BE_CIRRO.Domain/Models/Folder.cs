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
    public class Folder : ModelBase
    {
        [Key]
        public Guid FolderId { get; set; }   

        [Required]
        [MaxLength(255)]
        public required string Name { get; set; }

        public Guid? ParentFolderId { get; set; }

        [ForeignKey(nameof(ParentFolderId))]
        public Folder? ParentFolder { get; set; }

        public Guid OwnerId { get; set; }


        [ForeignKey(nameof(OwnerId))]
        public required User Owner { get; set; }

    }
}
