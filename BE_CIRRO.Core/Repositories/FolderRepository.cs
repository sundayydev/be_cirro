using BE_CIRRO.Core.Configurations;
using BE_CIRRO.Domain.IRepositories;
using BE_CIRRO.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BE_CIRRO.Core.Repositories;

public class FolderRepository : GenericRepository<Folder>, IFolderRepository
{
    private new readonly AppDbContext _context;

    public FolderRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Folder>> GetByOwnerAsync(Guid ownerId)
    {
        return await _context.Folders
            .Where(f => f.OwnerId == ownerId)
            .Include(f => f.Owner)
            .ToListAsync();
    }

    public async Task<IEnumerable<Folder>> GetByParentAsync(Guid? parentId)
    {
        return await _context.Folders
            .Where(f => f.ParentFolderId == parentId)
            .Include(f => f.Owner)
            .ToListAsync();
    }
    public async Task<IEnumerable<Folder>> GetRootFoldersByUserAsync(Guid ownerId)
    {
        return await _context.Folders
            .Where(f => f.OwnerId == ownerId && f.ParentFolderId == null)
            .ToListAsync();
    }


}
