namespace _02._Application.Storage;


public interface IFileStorage
{
    Task<FileUploadResult> UploadAsync(
        Stream file,
        string fileName,
        string folder);

    Task DeleteAsync(string publicId);
}
