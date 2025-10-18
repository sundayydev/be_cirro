using BE_CIRRO.Core.Configurations;
using BE_CIRRO.Domain.IRepositories;
using BE_CIRRO.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = BE_CIRRO.Domain.Models.File;

namespace BE_CIRRO.Core.Repositories;

public class FileRepository : GenericRepository<File>, IFileRepository
{
    private new readonly AppDbContext _context;
    public FileRepository(AppDbContext context) : base(context)
    {
        this._context = context;
    }

    public async Task<IEnumerable<File>> GetByFolderAsync(Guid folderId)
    {
        return await _context.Files
            .Where(f => f.FolderId == folderId)
            .ToListAsync();
    }
    public async Task<IEnumerable<File>> GetByUserAsync(Guid userId)
    {
        return await _context.Files
            .Where(f => f.OwnerId == userId)
            .Include(f => f.Owner) 
            .ToListAsync();
    }
}
