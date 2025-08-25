using FiberCore;
using FiberCore.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var configuration = builder.Configuration;
var conStr = configuration.GetConnectionString("DefaultConnection");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<FiberCalculator>();
builder.Services.AddScoped<FibercalcState>();

//builder.Services.AddHttpClient();

await builder.Build().RunAsync();
