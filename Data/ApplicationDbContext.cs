using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkSpaceApi.Models;

namespace WorkSpaceApi.Data
{
    public class ApplicationDbContext:IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options)
        {
            
        }
        public DbSet<CheckIns> CheckIns { get; set; }
        public DbSet<Order> Order { get; set; }
    }
}
