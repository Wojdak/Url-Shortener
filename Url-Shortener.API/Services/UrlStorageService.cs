using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Security.Cryptography;
using System.Text;

namespace Url_Shortener.API.Services
{
    public class UrlStorageService : IUrlStorageService
    {
        private BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        public UrlStorageService(BlobServiceClient blobServiceClienst, IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClienst;
            _configuration = configuration;
        }

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

        public async Task<string> SaveUrlMappingAsync(string longUrl)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_configuration["containerName"]);

            string shortUrl = GenerateShortUrl(longUrl);

            var blobClient = containerClient.GetBlobClient(shortUrl);

            var bytes = Encoding.UTF8.GetBytes(longUrl);

            using (var stream = new MemoryStream(bytes))
            {
                await blobClient.UploadAsync(stream, true);
            }

            return shortUrl;
        }

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

    }
}
