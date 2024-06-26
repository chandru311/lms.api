using lms.api.Models;
using Microsoft.EntityFrameworkCore;

namespace lms.api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Usermaster> Usermasters { get; set; }
        public DbSet<Employees> Employees { get; set; }
        public DbSet<Managers> Managers { get; set; }
        public DbSet<Departments> Departments { get; set; }
    }
}
