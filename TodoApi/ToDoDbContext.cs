using Microsoft.EntityFrameworkCore;
using TodoApi;




public class ToDoDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }

    public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>().ToTable("Items");
    }
}
