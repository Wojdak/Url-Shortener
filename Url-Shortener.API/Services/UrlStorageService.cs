using Azure;
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
        private BlobContainerClient containerClient;

        #region Constructor
        public UrlStorageService(BlobServiceClient blobServiceClienst, IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClienst;
            _configuration = configuration;
            containerClient = _blobServiceClient.GetBlobContainerClient(_configuration["containerName"]);
        }

        #endregion

        #region Public Methods
        public async Task<string> GetOriginalUrlAsync(string shortUrl)
        {
            if (!await containerClient.ExistsAsync())
                throw new Exception("There was an error while accessing the short URL");

            var blobClient = containerClient.GetBlobClient(shortUrl);

            if (!await blobClient.ExistsAsync())
                throw new Exception("URL not found");

            BlobDownloadInfo download = await blobClient.DownloadAsync();
            using (StreamReader reader = new StreamReader(download.Content))
            {
                return await reader.ReadToEndAsync();
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
