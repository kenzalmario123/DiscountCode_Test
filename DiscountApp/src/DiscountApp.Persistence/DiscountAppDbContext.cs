using DiscountApp.Domain.Entities;
using DiscountApp.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiscountApp.Persistence;

public class DiscountAppDbContext : DbContext, IDiscountAppDbContext
{
    public DiscountAppDbContext()
    {
    }

    public DiscountAppDbContext(DbContextOptions<DiscountAppDbContext> options)
        : base(options)
    {
    }

    public DbSet<DiscountCode> DiscountCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "1.1.1-servicing");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DiscountAppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}