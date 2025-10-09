using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BE_CIRRO.Domain.Models
{
    public class File
    {
        [Key]
        public Guid FileId { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(500)]
        public required string FilePath { get; set; }

        public int Size { get; set; }  

        [MaxLength(100)]
        public string? FileType { get; set; } 

        public Guid FolderId { get; set; }

        [ForeignKey(nameof(FolderId))]
        public required Folder Folder { get; set; }

        public Guid OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public required User Owner { get; set; }
    }
}
