using _02._Application.Storage;
using Microsoft.Extensions.Hosting;
namespace _03._Infrastructure.Storage;
public sealed class LocalFileStorage(IHostEnvironment environment) : IFileStorage
{
    public async Task<FileUploadResult> UploadAsync(Stream file, string fileName, string folder)
    {
        var (data, extension) = await ImageValidation.ReadAsync(file);
        await using var validated = data;
        var id = Guid.NewGuid().ToString("N") + extension;
        var directory = Path.Combine(environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(directory);
        await using var output = File.Create(Path.Combine(directory, id));
        await validated.CopyToAsync(output);
        return new("/uploads/" + id, id);
    }
    public Task DeleteAsync(string publicId)
    {
        if (Path.GetFileName(publicId) != publicId || publicId.Contains('/') || publicId.Contains('\\')) throw new ArgumentException("Invalid image identifier.");
        File.Delete(Path.Combine(environment.ContentRootPath, "uploads", publicId));
        return Task.CompletedTask;
    }
}
