using CryptoAlertsBackend.Models;
using CryptoAlertsBackend.Workers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<EndpointContext>(
        options => options.UseSqlServer(builder.Configuration
        .GetConnectionString("DevConnection"))
    );
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddHostedService<TestBgService>();
builder.Services.AddScoped<AssetService>();

var app = builder.Build();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
