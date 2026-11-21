namespace RealEstateWeb.Services;

public class PropertyImageStorage
{
    private readonly IWebHostEnvironment _environment;
    public PropertyImageStorage(IWebHostEnvironment environment) => _environment = environment;

    public async Task<string?> SaveAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        if (file.Length > 5 * 1024 * 1024) throw new InvalidOperationException("Ảnh tối đa 5 MB.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var header = new byte[12];
        using var source = file.OpenReadStream();
        var read = await source.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
        var jpeg = read >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff;
        var png = read >= 8 && header.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var webp = read >= 12 && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF"
            && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP";
        if (!((extension is ".jpg" or ".jpeg") && jpeg || extension == ".png" && png || extension == ".webp" && webp))
            throw new InvalidOperationException("Chọn ảnh JPG, JPEG, PNG hoặc WebP hợp lệ.");
        var name = Guid.NewGuid().ToString("N") + extension;
        var directory = Path.Combine(_environment.WebRootPath, "uploads", "properties");
        Directory.CreateDirectory(directory);
        using var destination = File.Create(Path.Combine(directory, name));
        await destination.WriteAsync(header.AsMemory(0, read));
        await source.CopyToAsync(destination);
        return "/uploads/properties/" + name;
    }

    public bool Exists(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        var prefixes = new[] { "/user/images/", "/uploads/properties/" };
        var prefix = prefixes.FirstOrDefault(p => url.StartsWith(p, StringComparison.Ordinal));
        if (prefix == null) return false;
        var name = url[prefix.Length..];
        if (name != Path.GetFileName(name) || name.Contains('\\') || name is "." or "..") return false;
        var ext = Path.GetExtension(name).ToLowerInvariant();
        return new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext)
            && File.Exists(Path.Combine(_environment.WebRootPath, prefix.Trim('/').Replace('/', Path.DirectorySeparatorChar), name));
    }
}
