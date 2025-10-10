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

public class FileVersionRepository : GenericRepository<FileVersion>, IFileVersionRepository
{
    private new readonly AppDbContext _context;
    public FileVersionRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FileVersion>> GetByFileAsync(Guid fileId)
    {
        return await _context.FileVersions
               .Where(v => v.FileId == fileId)
               .OrderByDescending(v => v.CreatedAt)
               .ToListAsync();
    }

    public async Task<FileVersion?> GetLatestVersionAsync(Guid fileId)
    {
        return await _context.FileVersions
            .Where(v => v.FileId == fileId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync();
    }
}
