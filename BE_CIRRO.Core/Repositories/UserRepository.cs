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

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private new readonly AppDbContext _context;
    public UserRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
}
