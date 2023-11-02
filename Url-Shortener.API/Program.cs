
using Azure.Storage.Blobs;
using System.Text;
using Url_Shortener.API.Services;

namespace Url_Shortener.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration["BlobStorageConnectionString"]));
            builder.Services.AddScoped<IUrlStorageService, UrlStorageService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAnyOrigin");

            app.UseAuthorization();

            app.MapPost("/shorten", async (string longUrl, IUrlStorageService _urlStorageService, HttpContext _httpContext) =>
            {
                if(!Uri.TryCreate(longUrl, UriKind.Absolute, out _))
                    return Results.BadRequest("URL is invalid");

                var shortUrl = await _urlStorageService.SaveUrlMappingAsync(longUrl);

                return Results.Ok($"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}/{shortUrl}");
            });

            app.MapGet("/{shortUrl}", async (string shortUrl, IUrlStorageService _urlStorageService) =>
            {
                string longUrl = await _urlStorageService.GetOriginalUrlAsync(shortUrl);

                if (string.IsNullOrEmpty(longUrl))
                    return Results.NotFound("URL not found");

                return Results.Redirect(longUrl);
            });

            app.Run();

        }
    }
}