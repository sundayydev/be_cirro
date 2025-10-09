using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.Folder;

// DTO dùng để cập nhật Folder
public class FolderUpdateDto
{
    [Required]
    public Guid FolderId { get; set; }
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
}
