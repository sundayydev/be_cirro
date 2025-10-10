using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.DTOs.File;

public class UpdateFileDto
{
    public string Name { get; set; } = default!;
    public Guid? FolderId { get; set; }

}
