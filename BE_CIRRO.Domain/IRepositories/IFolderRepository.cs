using BE_CIRRO.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Domain.IRepositories;

public interface IFolderRepository : IRepository<Models.Folder>
{
    Task<IEnumerable<Folder>> GetByOwnerAsync(Guid ownerId);
    Task<IEnumerable<Folder>> GetByParentAsync(Guid? parentId);
}
