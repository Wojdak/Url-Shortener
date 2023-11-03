using Azure.Storage.Blobs;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Url_Shortener.API.Models;

namespace Url_Shortener.API.Services
{
    public class UrlManager : IUrlManager
    {
        public string shortUrl { get; set; } = string.Empty;
        private IUrlStorageService _urlStorageService;

        public UrlManager(IUrlStorageService urlStorageService)
        {
            _urlStorageService = urlStorageService;
        }

        public async Task<string> ShortenUrlAsync(ShortenUrlRequest request)
        {
            if (!Uri.TryCreate(request.LongUrl, UriKind.Absolute, out _))
            {
                throw new ArgumentException("URL is invalid");
            }

            if (!string.IsNullOrEmpty(request.CustomUrl))
            {
                if(IsValidCustomUrl(request.CustomUrl))
                {
                    // Check if the CustomUrl already exists.
                    bool exists = await _urlStorageService.CheckIfShortUrlExistsAsync(request.CustomUrl);
                    if (exists)
                        throw new ArgumentException("The custom URL is already in use.");

                    return request.CustomUrl;
                } 
                else
                {
                    throw new ArgumentException("The custom URL must contain only letters and numbers.");
                }
            }

            var shortUrlHash = GenerateShortUrl(request.LongUrl);

            return shortUrlHash;

        }

        private bool IsValidCustomUrl(string customUrl)
        {
            Regex validUrlPattern = new Regex("^[a-zA-Z0-9]*$");
            return validUrlPattern.IsMatch(customUrl);
        }

        private string GenerateShortUrl(string longUrl)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(longUrl));
                StringBuilder _builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    _builder.Append(bytes[i].ToString("x2"));
                }

                return _builder.ToString().Substring(0, 7);
            }
        }
    }
}
