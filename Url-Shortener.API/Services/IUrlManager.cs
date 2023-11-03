using Url_Shortener.API.Models;

namespace Url_Shortener.API.Services
{
    public interface IUrlManager
    {
        Task<string> ShortenUrlAsync(ShortenUrlRequest request);
    }
}
