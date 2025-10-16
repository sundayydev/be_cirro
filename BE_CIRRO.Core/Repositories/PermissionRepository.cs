using BE_CIRRO.Core.Configurations;
using BE_CIRRO.Domain.IRepositories;
using BE_CIRRO.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Core.Repositories;

public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
{
    private new readonly AppDbContext _context;
    public PermissionRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Permission>> GetByFileIdAsync(Guid fileId)
    {
        return await _context.Permissions
                .Where(p => p.FileId == fileId)
                .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Permissions
                .Where(p => p.UserId == userId)
                .Include(p => p.File)
                .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetByFolderIdAsync(Guid folderId)
    {
        return await _context.Permissions
                .Where(p => p.FolderId == folderId)
                .ToListAsync();
    }
}
