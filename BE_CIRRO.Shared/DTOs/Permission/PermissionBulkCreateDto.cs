using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.Permission;

public class PermissionBulkCreateDto
{
    public List<Guid> UserIds { get; set; } = new List<Guid>();
    public Guid FileId { get; set; }
    public Guid FolderId { get; set; }
    public string PermissionType { get; set; } = default!;
}
