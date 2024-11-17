using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Microsoft.AspNetCore.Builder;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<EndpointContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection"))
);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
//builder.Services.AddHostedService<TestBgService>();
builder.Services.AddScoped<AssetService>();

var app = builder.Build();

// Use Developer Exception Page in Development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Middleware
app.UseCors("AllowAll");
app.UseHttpsRedirection(); // Enables HTTPS redirection if needed

// Important: Register routes defined by controllers
app.MapControllers();

app.Run();
