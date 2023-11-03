using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace Url_Shortener.API.Services
{
    /// <summary>
    /// Service for storing and retrieving URL mappings in Azure Blob Storage.
    /// </summary>
    public class UrlStorageService : IUrlStorageService
    {
        private BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private BlobContainerClient containerClient;
        private readonly IMemoryCache _cache;
        #region Constructor
        public UrlStorageService(BlobServiceClient blobServiceClienst, IConfiguration configuration, IMemoryCache cache)
        {
            _blobServiceClient = blobServiceClienst;
            _configuration = configuration;
            containerClient = _blobServiceClient.GetBlobContainerClient(_configuration["containerName"]);
            _cache = cache;
        }

        #endregion

        #region Public Methods
        public async Task<string> GetOriginalUrlAsync(string shortUrl)
        {
            // Check if the URL is in the cache
            if (_cache.TryGetValue(shortUrl, out string longUrl))
            {
                return longUrl;
            }

            if (!await containerClient.ExistsAsync())
                throw new Exception("There was an error while accessing the short URL");

            var blobClient = containerClient.GetBlobClient(shortUrl);

            if (!await blobClient.ExistsAsync())
                throw new Exception("URL not found");

            BlobDownloadInfo download = await blobClient.DownloadAsync();
            using (StreamReader reader = new StreamReader(download.Content))
            {
                longUrl = await reader.ReadToEndAsync();

                // Store in cache with a sliding expiration of 5 minutes
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5),
                    Size = 1
                };
                _cache.Set(shortUrl, longUrl, cacheEntryOptions);

                return longUrl;
            }
        }

        public async Task SaveUrlMappingAsync(string shortUrl, string longUrl)
        {
            try
            {
                var blobClient = containerClient.GetBlobClient(shortUrl);

                var bytes = Encoding.UTF8.GetBytes(longUrl);

                using (var stream = new MemoryStream(bytes))
                {
                    await blobClient.UploadAsync(stream, true);
                }
            }
            catch (RequestFailedException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> CheckIfShortUrlExistsAsync(string shortUrl)
        {
            var blobClient = containerClient.GetBlobClient(shortUrl);
            return await blobClient.ExistsAsync();
        }

        #endregion
    }
}
