using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Repository
{
    public class AuthContext : DbContext
    {
        public AuthContext(DbContextOptions<AuthContext> options) : base(options)
        {
        }

        public DbSet<Credential> Users { get; set; }
    }
}