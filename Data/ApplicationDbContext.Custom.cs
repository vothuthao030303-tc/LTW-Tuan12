using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Models;

namespace RealEstateWeb.Data;

public partial class ApplicationDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Property>()
            .HasIndex(x => new { x.Status, x.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Properties_Status_CreatedAt");
    }
}
