using JwtApiSample.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtApiSample.Data.Context
{
    public class MyWorldDbContext : DbContext
    {
        public MyWorldDbContext(DbContextOptions<MyWorldDbContext> options) : base(options)
        {

        }

        public DbSet<User> User { get; set; }
    }
}