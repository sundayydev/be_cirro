using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Domain.IRepositories;

public interface IFileRepository : IRepository<Models.File>
{
    Task<IEnumerable<Models.File>> GetByFolderAsync(Guid folderId);
}
