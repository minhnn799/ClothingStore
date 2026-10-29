using _02._Application.Common;
namespace _03._Infrastructure.Storage;
internal static class ImageValidation
{
    public static async Task<(MemoryStream Stream, string Extension)> ReadAsync(Stream source)
    {
        var data = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = await source.ReadAsync(buffer)) > 0)
        {
            if (data.Length + read > 5_000_000) { data.Dispose(); throw new StoreException("Image exceeds 5 MB."); }
            await data.WriteAsync(buffer.AsMemory(0, read));
        }
        var bytes = data.ToArray();
        var extension = bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}) ? ".png"
            : bytes.Length >= 3 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255 ? ".jpg"
            : bytes.Length >= 12 && System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP" ? ".webp" : null;
        if (extension is null) { data.Dispose(); throw new StoreException("Only PNG, JPEG, and WebP images are accepted."); }
        data.Position = 0;
        return (data, extension);
    }
}
