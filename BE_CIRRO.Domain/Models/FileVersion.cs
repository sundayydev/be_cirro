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
    public class FileVersion : ModelBase
    {
        [Key]
        public Guid VersionId { get; set; }
        

        public Guid FileId { get; set; }
        

        [ForeignKey(nameof(FileId))]
        public File File { get; set; }

        [Required]
        public int VersionNumber { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; }

        public int Size { get; set; }
    }
}
