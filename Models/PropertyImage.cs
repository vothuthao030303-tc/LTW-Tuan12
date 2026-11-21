using System.ComponentModel.DataAnnotations;

namespace RealEstateWeb.Models;

public partial class PropertyImage
{
    public int PropertyImageId { get; set; }
    public int PropertyId { get; set; }
    public virtual Property? Property { get; set; }

    [Required, StringLength(255)]
    public string ImageUrl { get; set; } = "";

    public bool IsMain { get; set; }
}
