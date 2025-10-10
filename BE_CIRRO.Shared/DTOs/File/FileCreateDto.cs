using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.File;

public class FileCreateDto
{
    public required IFormFile File { get; set; }
    public Guid FolderId { get; set; }
    public Guid OwnerId { get; set; }
}
