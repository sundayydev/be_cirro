using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.Permission;

public class PermissionDto
{
    public Guid PermissionId { get; set; }
    public Guid UserId { get; set; }
    public Guid? FileId { get; set; }
    public Guid? FolderId { get; set; }
    public string PermissionType { get; set; } = default!;
    public string OwnerName { get; set; }
}
