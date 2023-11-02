using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Url_Shortener.Web;

namespace Url_Shortener.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            if (builder.HostEnvironment.IsDevelopment())
            {
                // Local development configuration
                builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7029") });
            }
            else
            {
                // Production configuration
                builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            }


            await builder.Build().RunAsync();
        }
    }
}