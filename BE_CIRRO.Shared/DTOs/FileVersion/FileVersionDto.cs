using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.FileVersion;

public class FileVersionDto
{
    public Guid VersionId { get; set; }
    public Guid FileId { get; set; }
    public string VersionNumber { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public int Size { get; set; }
    public DateTime CreatedAt { get; set; }
}
