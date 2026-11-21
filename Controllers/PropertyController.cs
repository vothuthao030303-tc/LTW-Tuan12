using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.ViewModels;

namespace RealEstateWeb.Controllers;

public class PropertyController : Controller
{
    private readonly ApplicationDbContext _db;

    public PropertyController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? keyword, int? typeId, int? locationId,
        decimal? minPrice, decimal? maxPrice, int page = 1)
    {
        page = Math.Max(1, page);
        const int pageSize = 6;
        var query = _db.Properties.AsNoTracking()
            .Include(property => property.PropertyType)
            .Include(property => property.Location)
            .Where(property => property.Status == "Approved");

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(property => property.Title.Contains(keyword) || property.Address.Contains(keyword));
        }
        if (typeId.HasValue)
        {
            query = query.Where(property => property.PropertyTypeId == typeId);
        }
        if (locationId.HasValue)
        {
            query = query.Where(property => property.LocationId == locationId);
        }
        if (minPrice.HasValue)
        {
            query = query.Where(property => property.Price >= minPrice);
        }
        if (maxPrice.HasValue)
        {
            query = query.Where(property => property.Price <= maxPrice);
        }

        var count = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize));
        page = Math.Min(page, totalPages);
        return View(new PropertyListViewModel
        {
            Items = await query.OrderByDescending(property => property.CreatedAt)
                .ThenByDescending(property => property.PropertyId)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(),
            Types = await _db.PropertyTypes.AsNoTracking().ToListAsync(),
            Locations = await _db.Locations.AsNoTracking().ToListAsync(),
            Keyword = keyword,
            TypeId = typeId,
            LocationId = locationId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Page = page,
            TotalPages = totalPages,
            TotalItems = count
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var property = await _db.Properties.AsNoTracking()
            .Include(item => item.PropertyType)
            .Include(item => item.Location)
            .Include(item => item.User)
            .Include(item => item.Images)
            .FirstOrDefaultAsync(item => item.PropertyId == id && item.Status == "Approved");

        if (property == null)
        {
            return NotFound();
        }

        var gallery = new List<string>();
        if (!string.IsNullOrWhiteSpace(property.MainImage))
        {
            gallery.Add(property.MainImage);
        }
        gallery.AddRange(property.Images.OrderByDescending(image => image.IsMain)
            .ThenBy(image => image.PropertyImageId)
            .Select(image => image.ImageUrl)
            .Where(url => !string.IsNullOrWhiteSpace(url)));
        gallery = gallery.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (gallery.Count == 0)
        {
            gallery.Add("/user/images/hotel_feture_1.jpg");
        }

        var related = await _db.Properties.AsNoTracking()
            .Include(item => item.PropertyType)
            .Include(item => item.Location)
            .Where(item => item.Status == "Approved" && item.PropertyId != id
                && (item.PropertyTypeId == property.PropertyTypeId || item.LocationId == property.LocationId))
            .OrderByDescending(item => item.PropertyTypeId == property.PropertyTypeId)
            .ThenByDescending(item => item.LocationId == property.LocationId)
            .ThenByDescending(item => item.CreatedAt)
            .ThenByDescending(item => item.PropertyId)
            .Take(3).ToListAsync();

        return View(new PropertyDetailViewModel
        {
            Property = property,
            GalleryUrls = gallery,
            RelatedProperties = related
        });
    }
}
