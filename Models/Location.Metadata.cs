using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace RealEstateWeb.Models;

[ModelMetadataType(typeof(LocationMetadata))]
public partial class Location
{
}

public class LocationMetadata
{
    [Display(Name = "Mã khu vực")]
    public int LocationId { get; set; }

    [Display(Name = "Tỉnh/thành phố")]
    [Required(ErrorMessage = "Vui lòng nhập tỉnh/thành phố.")]
    [StringLength(100, ErrorMessage = "Tỉnh/thành phố không được quá 100 ký tự.")]
    public string City { get; set; } = "";

    [Display(Name = "Khu vực")]
    [StringLength(100, ErrorMessage = "Khu vực không được quá 100 ký tự.")]
    public string? District { get; set; }
}
