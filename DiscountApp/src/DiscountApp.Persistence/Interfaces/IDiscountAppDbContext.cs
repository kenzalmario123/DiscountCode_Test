using DiscountApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscountApp.Persistence.Interfaces;

public interface IDiscountAppDbContext
{
    DbSet<DiscountCode> DiscountCodes { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}