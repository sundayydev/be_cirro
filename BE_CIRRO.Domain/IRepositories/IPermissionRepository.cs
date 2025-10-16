using BE_CIRRO.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Domain.IRepositories;

public interface IPermissionRepository : IRepository<Models.Permission>
{
    Task<IEnumerable<Permission>> GetByFileIdAsync(Guid fileId);
    Task<IEnumerable<Permission>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Permission>> GetByFolderIdAsync(Guid folderId);
}
