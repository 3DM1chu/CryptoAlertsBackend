using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string SQL_SERVER_IP = Environment.GetEnvironmentVariable("DB_IP");
string SQL_DATABASE_NAME = Environment.GetEnvironmentVariable("SQL_DATABASE_NAME");
string DB_SA_PASSWD = Environment.GetEnvironmentVariable("DB_SA_PASSWD");
string CONNECTION_STRING = $"Server={SQL_SERVER_IP};Database={SQL_DATABASE_NAME};User=sa;Password={DB_SA_PASSWD};TrustServerCertificate=True;MultipleActiveResultSets=True";

// Add services to the container
builder.Services.AddDbContext<EndpointContext>(
    options => options.UseSqlServer(CONNECTION_STRING)
    //"Server=localhost,25557;Database=TokenManagement;User ID=sa;Password=Password_123;TrustServerCertificate=True;MultipleActiveResultSets=True")
);

builder.Services.AddControllers();
/*
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});*/

builder.Services.AddScoped<AssetService>();

var app = builder.Build();

// Use Developer Exception Page in Development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Middleware
app.UseCors("AllowAll");

// Important: Register routes defined by controllers
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EndpointContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception) { }
}

app.Run();
