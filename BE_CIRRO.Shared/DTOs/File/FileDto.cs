using BE_CIRRO.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.File;

public class FileDto : DTOBase
{
    public Guid FileId { get; set; }
    public string Name { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public string FileType { get; set; } = default!;
    public int Size { get; set; }
    public Guid FolderId { get; set; }
    public Guid OwnerId { get; set; }
}
