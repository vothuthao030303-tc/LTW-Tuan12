using RealEstateWeb.Models;

namespace RealEstateWeb.ViewModels;

public class AdminPropertyListViewModel
{
    public List<Property> Items { get; set; } = new();
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
}
