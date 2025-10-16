using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.FileVersion;

public class FileVersionUpdateDto
{
    public int? VersionNumber { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
