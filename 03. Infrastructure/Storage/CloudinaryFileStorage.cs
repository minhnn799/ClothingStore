using _02._Application.Storage;

namespace _03._Infrastructure.Storage;

public class CloudinaryFileStorage : IFileStorage
{
    public Task DeleteAsync(string publicId)
    {
        throw new NotImplementedException();
    }

    public Task<FileUploadResult> UploadAsync(Stream file, string fileName, string folder)
    {
        throw new NotImplementedException();
    }
}
