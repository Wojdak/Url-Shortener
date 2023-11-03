namespace Url_Shortener.API.Services
{
    public interface IUrlStorageService
    {
        Task<string?> SaveUrlMappingAsync(string longUrl, string? customUrl);
        Task<string?> GetOriginalUrlAsync(string shortUrl);
        string GenerateShortUrl(string longUrl);
    }
}
