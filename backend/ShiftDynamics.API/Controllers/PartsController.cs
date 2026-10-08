using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/parts")]
[Authorize(Roles = "Customer,Storekeeper,Manager,Admin,ServiceAdvisor,Mechanic")]
public class PartsController(ShiftDynamicsDbContext db, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List()
    {
        var parts = await db.Parts.AsNoTracking().Where(p => p.IsActive).OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.PartNumber, p.Name, p.Description, p.Category, p.Compatibility, p.ImageUrl,
                IsAvailable = p.Inventory != null && p.Inventory.OnHandQty > p.Inventory.ReservedQty }).ToListAsync();
        return Ok(ApiResponse<object>.Ok(parts));
    }

    [HttpPost("{id:guid}/image")]
    [Authorize(Policy = "Storekeeper")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<object>>> UploadImage(Guid id, [FromForm] IFormFile file)
    {
        var part = await db.Parts.FindAsync(id) ?? throw new NotFoundException("Part not found.");
        if (file.Length is <= 0 or > 5 * 1024 * 1024)
            throw new ValidationException("Choose a JPG, PNG or WebP image no larger than 5 MB.");
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer);
        var bytes = buffer.ToArray();
        var extension = DetectImage(bytes) ?? throw new ValidationException("Only JPG, PNG and WebP images are supported.");
        var folder = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "parts");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(folder, name);
        await System.IO.File.WriteAllBytesAsync(destination, bytes);
        try
        {
            part.ImageUrl = $"/uploads/parts/{name}";
            part.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        catch { System.IO.File.Delete(destination); throw; }
        return Ok(ApiResponse<object>.Ok(new { part.Id, part.ImageUrl }, "Part image saved."));
    }

    private static string? DetectImage(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff) return ".jpg";
        if (bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return ".png";
        if (bytes.Length >= 12 && System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP") return ".webp";
        return null;
    }
}