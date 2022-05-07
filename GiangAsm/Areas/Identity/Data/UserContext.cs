using GiangAsm.Areas.Identity.Data;
using GiangAsm.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiangAsm.Areas.Identity.Data;

public class UserContext : IdentityDbContext<AppUser>
{
    public UserContext(DbContextOptions<UserContext> options)
        : base(options)
    {
    }
    public DbSet<Store> Store { get; set; }
    public DbSet<Book> Book { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<OrderDetail> OrderDetail { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>()
               .HasOne<Store>(au => au.Store)
               .WithOne(st => st.User)
               .HasForeignKey<Store>(st => st.UserId);

        builder.Entity<Book>()
            .HasOne<Store>(b => b.Store)
            .WithMany(st => st.Books)
            .HasForeignKey(b => b.StoreId);

        builder.Entity<Order>()
            .HasOne<AppUser>(o => o.User)
            .WithMany(ap => ap.Orders)
            .HasForeignKey(o => o.UserId);

        builder.Entity<OrderDetail>()
            .HasKey(od => new { od.OrderId, od.BookIsbn });
        builder.Entity<OrderDetail>()
            .HasOne<Order>(od => od.Order)
            .WithMany(or => or.OrderDetails)
            .HasForeignKey(od => od.OrderId);
        builder.Entity<OrderDetail>()
            .HasOne<Book>(od => od.Book)
            .WithMany(b => b.OrderDetails)
            .HasForeignKey(od => od.BookIsbn);

    }
}
