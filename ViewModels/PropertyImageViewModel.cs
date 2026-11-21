using System.ComponentModel.DataAnnotations;

namespace RealEstateWeb.ViewModels;

public class PropertyImageViewModel
{
    public int PropertyImageId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn bất động sản.")]
    public int PropertyId { get; set; }
    [StringLength(255)] public string? ImageUrl { get; set; }
    public IFormFile? Image { get; set; }
    public bool IsMain { get; set; }
}
