using aws_bucket.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace aws_bucket.Data
{
    public class DbContextClass : IdentityDbContext<IdentityUser>
    {
        protected readonly IConfiguration Configuration;
        public DbContextClass(DbContextOptions configuration) : base(configuration)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
        public DbSet<UserInfo> UserInfos { get; set; }
        public DbSet<Order> Orders { get; set; }

    }
}
