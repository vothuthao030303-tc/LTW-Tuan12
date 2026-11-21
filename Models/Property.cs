using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWeb.Models;

public partial class Property
{
    public int PropertyId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Range(1, 100000)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Area { get; set; }

    [Required, StringLength(255)]
    public string Address { get; set; } = "";

    public int PropertyTypeId { get; set; }
    public virtual PropertyType? PropertyType { get; set; }

    public int LocationId { get; set; }
    public virtual Location? Location { get; set; }

    public int UserId { get; set; }
    public virtual User? User { get; set; }

    [Range(0, 50)]
    public int Bedrooms { get; set; }

    [Range(0, 50)]
    public int Bathrooms { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = "Pending";

    [StringLength(255)]
    public string? MainImage { get; set; }

    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public virtual ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
}
