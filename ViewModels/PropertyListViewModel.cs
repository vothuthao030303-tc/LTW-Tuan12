using RealEstateWeb.Models;

namespace RealEstateWeb.ViewModels;

public class PropertyListViewModel
{
    public List<Property> Items { get; set; } = new();
    public List<PropertyType> Types { get; set; } = new();
    public List<Location> Locations { get; set; } = new();
    public string? Keyword { get; set; }
    public int? TypeId { get; set; }
    public int? LocationId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; }
}
