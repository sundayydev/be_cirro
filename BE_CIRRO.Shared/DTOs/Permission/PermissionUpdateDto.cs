using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.Permission;

public class PermissionUpdateDto
{
    public string PermissionType { get; set; } = "read";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
