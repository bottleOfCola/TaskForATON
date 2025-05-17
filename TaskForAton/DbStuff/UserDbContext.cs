using Microsoft.EntityFrameworkCore;
using TaskForATON.DbStuff.Models;

namespace TaskForATON.DbStuff;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<UserModel> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserModel>().HasKey(x => x.Guid);
    }
}