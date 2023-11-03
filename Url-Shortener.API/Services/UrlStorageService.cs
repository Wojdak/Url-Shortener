using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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

        #region Constructor
        public UrlStorageService(BlobServiceClient blobServiceClienst, IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClienst;
            _configuration = configuration;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the original URL based on a given short URL.
        /// </summary>
        /// <param name="shortUrl">The short URL identifier.</param>
        /// <returns>The original URL if found; otherwise, null.</returns>
        public async Task<string?> GetOriginalUrlAsync(string shortUrl)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_configuration["containerName"]);

            if (!await containerClient.ExistsAsync())
                return null;

            var blobClient = containerClient.GetBlobClient(shortUrl);

            if (!await blobClient.ExistsAsync())
                return null;

            BlobDownloadInfo download = await blobClient.DownloadAsync();
            using (StreamReader reader = new StreamReader(download.Content))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Saves the mapping of a long URL to a short URL in Blob Storage.
        /// </summary>
        /// <param name="longUrl">The original long URL.</param>
        /// <param name="customUrl">An optional custom short URL identifier.</param>
        /// <returns>The short URL if successful; otherwise, null.</returns>
        public async Task<string?> SaveUrlMappingAsync(string longUrl, string? customUrl)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_configuration["containerName"]);

            string shortUrl = customUrl ?? GenerateShortUrl(longUrl); // Use customUrl if it exists, use randomly generated url

            var blobClient = containerClient.GetBlobClient(shortUrl);

            if (customUrl != null && await blobClient.ExistsAsync()) // Return null if customUrl already exists in Blob Storage
            {
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(longUrl);

            using (var stream = new MemoryStream(bytes))
            {
                await blobClient.UploadAsync(stream, true);
            }

            return shortUrl;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Generates a short URL identifier from a long URL using SHA256 hashing.
        /// </summary>
        /// <param name="longUrl">The long URL to be shortened.</param>
        /// <returns>A short URL identifier.</returns>
        public string GenerateShortUrl(string longUrl)
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

        #endregion
    }
}
