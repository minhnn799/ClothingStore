using _02._Application.Storage;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
namespace _03._Infrastructure.Storage;
public sealed class CloudinaryFileStorage : IFileStorage
{
    private readonly Cloudinary cloudinary;
    public CloudinaryFileStorage(IConfiguration configuration)
    {
        var name = configuration["Cloudinary:CloudName"];
        var key = configuration["Cloudinary:ApiKey"];
        var secret = configuration["Cloudinary:ApiSecret"];
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("Configure Cloudinary credentials before selecting Cloudinary storage.");
        cloudinary = new Cloudinary(new Account(name, key, secret)) { Api = { Secure = true } };
    }
    public async Task<FileUploadResult> UploadAsync(Stream file, string fileName, string folder)
    {
        var (data, extension) = await ImageValidation.ReadAsync(file);
        await using var validated = data;
        var result = await cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(Guid.NewGuid().ToString("N") + extension, validated),
            Folder = "clothes-store/products",
            UseFilename = false,
            UniqueFilename = true
        });
        if (result.Error is not null || result.SecureUrl is null) throw new InvalidOperationException("Image upload failed.");
        return new(result.SecureUrl.ToString(), result.PublicId);
    }
    public async Task DeleteAsync(string publicId)
    {
        var result = await cloudinary.DestroyAsync(new DeletionParams(publicId));
        if (result.Error is not null) throw new InvalidOperationException("Image deletion failed.");
    }
}
