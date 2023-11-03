namespace Url_Shortener.API.Models
{
    public class ShortenUrlRequest
    {
        public string LongUrl { get; set; } = string.Empty;
        public string? CustomUrl { get; set; } = string.Empty;
    }
}
