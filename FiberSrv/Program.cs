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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowWebAssembly");

app.MapControllers(); // Регистрация маршрутов для контроллеров

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
