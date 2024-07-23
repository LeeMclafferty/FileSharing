using Microsoft.EntityFrameworkCore;
using FileSharing.Models;

namespace FileSharing.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            :base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<FileSharing.Models.File> Files { get; set; }
    }
}
