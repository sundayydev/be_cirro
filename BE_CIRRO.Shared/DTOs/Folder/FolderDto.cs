
using BE_CIRRO.Shared.Common;

namespace BE_CIRRO.Shared.DTOs.Folder;

public class FolderDto : DTOBase
{
    public Guid FolderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public string OwnerId { get; set; } 
    public string OwnerName { get; set; } = string.Empty;
   
}
