using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.File;

public class FileUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? FolderId { get; set; }
}

public class FileMoveDto
{
    public Guid TargetFolderId { get; set; }
}

public class FileCopyDto
{
    public Guid TargetFolderId { get; set; }
    public string? NewName { get; set; }
}

public class FileSearchDto
{
    public string? SearchTerm { get; set; }
    public string? FileType { get; set; }
    public Guid? FolderId { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class FileShareDto
{
    public List<Guid> UserIds { get; set; } = new List<Guid>();
    public string PermissionType { get; set; } = "read";
}
