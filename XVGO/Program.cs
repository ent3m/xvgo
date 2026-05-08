using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using XVGO.Models;
using XVGO.Services;

namespace XVGO;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
            .AddScoped<ConversionSettings>()
            .AddScoped<ISvgOptimizer, SvgOptimizer>()
            .AddScoped<ISvgConverter, SvgConverter>()
            .AddScoped<IHistoryManager, HistoryManager>()
            .AddScoped<IClipboardService, ClipboardService>()
            .AddFluentUIComponents();

        await builder.Build().RunAsync();
    }
}
