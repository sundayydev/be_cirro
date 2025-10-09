using BE_CIRRO.Core.Configurations;
using BE_CIRRO.Domain.IRepositories;
using BE_CIRRO.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = BE_CIRRO.Domain.Models.File;

namespace BE_CIRRO.Core.Repositories;

public class FileRepository : GenericRepository<File>, IFileRepository
{
    public FileRepository(AppDbContext context) : base(context)
    {

    }
}
