using RealEstateWeb.Models;

namespace RealEstateWeb.ViewModels;

public class PropertyDetailViewModel
{
    public Property Property { get; set; } = new();
    public List<string> GalleryUrls { get; set; } = new();
    public List<Property> RelatedProperties { get; set; } = new();
}
