using System.ComponentModel.DataAnnotations;

namespace RealEstateWeb.Models;

public partial class PropertyType
{
    public int PropertyTypeId { get; set; }

    [Required, StringLength(100)]
    public string TypeName { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
