using FiberSrv.Components;
using FiberSrv.Repositories;
using System.Data.Entity.Spatial;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Добавление сервисов в контейнер зависимостей
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddSingleton<CalcRepository>(sp => new CalcRepository(connectionString??""));

builder.Services.AddSingleton<MaterialRepository>(sp => new MaterialRepository(connectionString ?? ""));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebAssembly",
        builder =>
        {
            builder.WithOrigins("https://localhost:7136")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});


var app = builder.Build();

//TODO тест отображения графиков на сервере
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    
    app.UseHsts();
}


app.MapGet("/rand", async context =>
{
    string html = "<html><body><img src='random.png'></body></html>";
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

app.MapGet("/random.png", async context =>
{
    ScottPlot.Plot myPlot = new();
    double[] dataX = ScottPlot.Generate.Consecutive(100);
    double[] dataY = ScottPlot.Generate.RandomWalk(100);
    myPlot.Add.Scatter(dataX, dataY);

    byte[] imageBytes = myPlot.GetImageBytes(400, 300, ScottPlot.ImageFormat.Png);
    context.Response.ContentType = "image/png";
    await context.Response.Body.WriteAsync(imageBytes, 0, imageBytes.Length);
});

app.MapGet("/svg", async context =>
{
    ScottPlot.Plot myPlot = new();
    double[] dataX = ScottPlot.Generate.Consecutive(100);
    double[] dataY = ScottPlot.Generate.RandomWalk(100);
    myPlot.Add.Scatter(dataX, dataY);

    string svg = myPlot.GetSvgXml(600, 400);
    string html = $"<html><body>{svg}</body></html>";
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});



app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowWebAssembly");

app.MapControllers(); // Регистрация маршрутов для контроллеров

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
