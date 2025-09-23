using BE_CIRRO.Domain.Models;
using File = BE_CIRRO.Domain.Models.File;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Core.Configurations
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<FileVersion> FileVersions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Áp dụng các cấu hình từ assembly (nếu có)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

           
        }
    }
}
