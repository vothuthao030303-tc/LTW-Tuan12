using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Models;

namespace RealEstateWeb.Data;

// Mapping is aligned with Database/RealEstateWebDb.sql.
// This file is maintained by hand; see HUONG_DAN_EF_TUAN11.md for reverse engineering.
public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public virtual DbSet<User> Users => Set<User>();
    public virtual DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
    public virtual DbSet<Location> Locations => Set<Location>();
    public virtual DbSet<Property> Properties => Set<Property>();
    public virtual DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", "dbo", table =>
            {
                table.HasCheckConstraint("CK_Users_Role", "[Role] IN (N'Admin', N'User')");
                table.HasCheckConstraint("CK_Users_Status", "[Status] IN (N'Active', N'Inactive', N'Locked')");
            });
            entity.HasKey(x => x.UserId).HasName("PK_Users");
            entity.Property(x => x.UserId).UseIdentityColumn();
            entity.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PasswordHash).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(20);
            entity.Property(x => x.Address).HasMaxLength(255);
            entity.Property(x => x.Role).HasMaxLength(20).IsRequired().HasDefaultValue("User");
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Active");
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(7)").HasDefaultValueSql("SYSDATETIME()");
            entity.HasIndex(x => x.Email).IsUnique().HasDatabaseName("IX_Users_Email");
        });

        modelBuilder.Entity<PropertyType>(entity =>
        {
            entity.ToTable("PropertyTypes", "dbo");
            entity.HasKey(x => x.PropertyTypeId).HasName("PK_PropertyTypes");
            entity.Property(x => x.PropertyTypeId).UseIdentityColumn();
            entity.Property(x => x.TypeName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Locations", "dbo");
            entity.HasKey(x => x.LocationId).HasName("PK_Locations");
            entity.Property(x => x.LocationId).UseIdentityColumn();
            entity.Property(x => x.City).HasMaxLength(100).IsRequired();
            entity.Property(x => x.District).HasMaxLength(100);
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("Properties", "dbo", table =>
            {
                table.HasCheckConstraint("CK_Properties_Price", "[Price] >= 0");
                table.HasCheckConstraint("CK_Properties_Area", "[Area] BETWEEN 1 AND 100000");
                table.HasCheckConstraint("CK_Properties_Bedrooms", "[Bedrooms] BETWEEN 0 AND 50");
                table.HasCheckConstraint("CK_Properties_Bathrooms", "[Bathrooms] BETWEEN 0 AND 50");
                table.HasCheckConstraint("CK_Properties_Status", "[Status] IN (N'Pending', N'Approved', N'Rejected', N'Sold')");
            });
            entity.HasKey(x => x.PropertyId).HasName("PK_Properties");
            entity.Property(x => x.PropertyId).UseIdentityColumn();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Area).HasColumnType("decimal(10,2)");
            entity.Property(x => x.Address).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Bedrooms).HasDefaultValue(0);
            entity.Property(x => x.Bathrooms).HasDefaultValue(0);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Pending");
            entity.Property(x => x.MainImage).HasMaxLength(255);
            entity.Property(x => x.IsFeatured).HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(7)").HasDefaultValueSql("SYSDATETIME()");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(7)");
            entity.HasIndex(x => x.PropertyTypeId).HasDatabaseName("IX_Properties_PropertyTypeId");
            entity.HasIndex(x => x.LocationId).HasDatabaseName("IX_Properties_LocationId");
            entity.HasIndex(x => x.UserId).HasDatabaseName("IX_Properties_UserId");
            entity.HasOne(x => x.PropertyType).WithMany(x => x.Properties)
                .HasForeignKey(x => x.PropertyTypeId).OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Properties_PropertyTypes_PropertyTypeId");
            entity.HasOne(x => x.Location).WithMany(x => x.Properties)
                .HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Properties_Locations_LocationId");
            entity.HasOne(x => x.User).WithMany(x => x.Properties)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Properties_Users_UserId");
        });

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.ToTable("PropertyImages", "dbo");
            entity.HasKey(x => x.PropertyImageId).HasName("PK_PropertyImages");
            entity.Property(x => x.PropertyImageId).UseIdentityColumn();
            entity.Property(x => x.ImageUrl).HasMaxLength(255).IsRequired();
            entity.Property(x => x.IsMain).HasDefaultValue(false);
            entity.HasIndex(x => x.PropertyId).HasDatabaseName("IX_PropertyImages_PropertyId");
            entity.HasOne(x => x.Property).WithMany(x => x.Images)
                .HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PropertyImages_Properties_PropertyId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
