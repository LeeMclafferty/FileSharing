using Microsoft.EntityFrameworkCore;
using FileSharing.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace FileSharing.Data
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            :base(options)
        { }

        //public DbSet<ApplicationUser> ApplicationUsers {  get; set; }
        public DbSet<FileSharing.Models.File> Files { get; set; }
    }
}
