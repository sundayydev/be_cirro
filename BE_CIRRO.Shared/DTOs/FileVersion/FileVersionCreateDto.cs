using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.FileVersion;

public class FileVersionCreateDto
{
    public Guid FileId { get; set; }
    public IFormFile File { get; set; } = default!;
}
