using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


string SQL_SERVER_IP = Environment.GetEnvironmentVariable("DB_IP");
string SQL_DATABASE_NAME = Environment.GetEnvironmentVariable("SQL_DATABASE_NAME");
string DB_SA_PASSWD = Environment.GetEnvironmentVariable("DB_SA_PASSWD");
string DB_USER = Environment.GetEnvironmentVariable("DB_USER");
int DB_PORT = int.Parse(Environment.GetEnvironmentVariable("DB_PORT"));
string CONNECTION_STRING = $"server={SQL_SERVER_IP};port={DB_PORT};database={SQL_DATABASE_NAME};user={DB_USER};password={DB_SA_PASSWD}";

// Add services to the container
builder.Services.AddDbContext<EndpointContext>(
    options => options.UseMySQL(CONNECTION_STRING)
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
