
namespace BE_CIRRO.Shared.DTOs.Folder;

public class FolderDto
{
    public Guid FolderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
}
