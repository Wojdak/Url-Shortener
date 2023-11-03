namespace Url_Shortener.API.Services
{
    public interface IUrlStorageService
    {
        Task SaveUrlMappingAsync(string shortUrl, string longUrl);
        Task<string> GetOriginalUrlAsync(string shortUrl);
        Task<bool> CheckIfShortUrlExistsAsync(string shortUrl);
    }
}
