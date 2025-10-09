using BE_CIRRO.Core.Configurations;
using BE_CIRRO.Domain.IRepositories;
using BE_CIRRO.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Core.Repositories;

public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(AppDbContext context) : base(context)
    {
    }
}
