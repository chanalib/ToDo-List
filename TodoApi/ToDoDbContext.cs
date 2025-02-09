using Microsoft.EntityFrameworkCore;
//using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using TodoApi;




public class ToDoDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }
    //public DbSet<User> Users { get; set; } // הוספת DbSet למשתמשים

    public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>().ToTable("Items");
    }
}
