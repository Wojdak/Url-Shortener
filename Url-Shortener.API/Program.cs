
using Azure;
using Azure.Storage.Blobs;
using System.Text;
using Url_Shortener.API.Models;
using Url_Shortener.API.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            builder.Services.AddScoped<IUrlManager, UrlManager>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder =>
                {
                    builder.AllowAnyMethod()
                            .AllowAnyHeader()
                            .SetIsOriginAllowed(origin => true)
                            .AllowCredentials();
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

            app.UseRouting();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseBlazorFrameworkFiles();
            app.MapFallbackToFile("index.html");

            app.UseAuthorization();

            // Define the POST endpoint for shortening URLs.
            app.MapPost("/shorten", async (ShortenUrlRequest request, IUrlStorageService _urlStorageService, IUrlManager _urlManager, HttpContext _context) =>
            {
                // Create the short URL
                string shortUrl;

                try
                {
                    shortUrl = await _urlManager.ShortenUrlAsync(request);

                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                // Save the short URL to Azure Blob Storage
                try
                {
                    await _urlStorageService.SaveUrlMappingAsync(shortUrl, request.LongUrl);
                    return Results.Ok(new StringBuilder($"{_context.Request.Scheme}://{_context.Request.Host}/{shortUrl}").ToString());
                } 
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

            });

            // Define the GET endpoint for redirecting short URLs to their original URLs.
            app.MapGet("/{shortUrl}", async (string shortUrl, IUrlStorageService _urlStorageService) =>
            {
                // Retrieve the long URL form Azure Blob Storage
                string longUrl;

                try
                {
                    longUrl = await _urlStorageService.GetOriginalUrlAsync(shortUrl);
                } 
                catch (Exception ex)
                {
                    return Results.NotFound(ex.Message);
                }

                // Redirect to the long URL
                return Results.Redirect(longUrl);
            });

            app.Run();

        }
    }
}