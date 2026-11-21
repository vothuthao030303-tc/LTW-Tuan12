namespace RealEstateWeb.Models;

// SQL mapping is configured in ApplicationDbContext.
// MVC labels and validation are kept in Location.Metadata.cs.
public partial class Location
{
    public int LocationId { get; set; }
    public string City { get; set; } = "";
    public string? District { get; set; }
    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
