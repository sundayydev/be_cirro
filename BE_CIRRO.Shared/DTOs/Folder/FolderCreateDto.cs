using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.Folder;

public class FolderCreateDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty; 
    public Guid? ParentFolderId { get; set; }
    [Required] 
    public Guid OwnerId { get; set; }
}
