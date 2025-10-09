using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.Folder;

public class FolderTreeDto
{
    public Guid FolderId { get; set; }
    public string Name { get; set; } = string.Empty; public Guid? ParentFolderId { get; set; }
    public Guid OwnerId { get; set; }
    public List<FolderTreeDto> Children { get; set; } = new();
}
