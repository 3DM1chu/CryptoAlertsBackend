using CryptoAlertsBackend.Middlewares;
using CryptoAlertsBackend.Models;
using CryptoAlertsBackend.Workers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string SQL_SERVER_IP = Environment.GetEnvironmentVariable("DB_IP");
string SQL_DATABASE_NAME = Environment.GetEnvironmentVariable("SQL_DATABASE_NAME");
string DB_SA_PASSWD = Environment.GetEnvironmentVariable("DB_SA_PASSWD");
string DB_USER = Environment.GetEnvironmentVariable("DB_USER");
int DB_PORT = int.Parse(Environment.GetEnvironmentVariable("DB_PORT"));
string CONNECTION_STRING = $"server={SQL_SERVER_IP};port={DB_PORT};database={SQL_DATABASE_NAME};user={DB_USER};password={DB_SA_PASSWD}";

// Add services to the container
builder.Services.AddDbContext<EndpointContext>(options =>
    options.UseMySQL(
        CONNECTION_STRING,
        mysqlOptions =>
        {
            // Configure retry logic for transient faults
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5, // Number of retry attempts
                maxRetryDelay: TimeSpan.FromSeconds(5), // Maximum delay between retries
                errorNumbersToAdd: [1042, 1045] // Add specific MySQL error codes for retries
            );
        })
        .EnableDetailedErrors() // Provides detailed EF Core error messages
        .LogTo(Console.WriteLine, LogLevel.Error) // Logs EF Core events and SQL queries
);


builder.Services.AddLogging(logging =>
{
    logging.ClearProviders(); // Removes all default providers
    logging.AddConsole(options =>
    {
        options.IncludeScopes = true; // Optional: Includes scopes in logs for better context
        options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] "; // Adds a timestamp to console logs
    });

    // Configure log levels
    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning); // Logs SQL queries
    logging.AddFilter("Microsoft", LogLevel.Warning); // Suppress other noisy logs from Microsoft libraries
    logging.AddFilter("System", LogLevel.Warning); // Suppress system library logs
});


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
builder.Services.AddHostedService<CleanerService>();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = 1000; // Adjust as needed
    options.Limits.MaxConcurrentUpgradedConnections = 1000; // For WebSockets
    options.Limits.MaxRequestBodySize = 10 * 1024; // Optional: Set max request body size (in bytes)
});

var app = builder.Build();

// Use Developer Exception Page in Development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Middleware
app.UseMiddleware<TokenAuthHeaderMiddleware>();
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

ThreadPool.SetMinThreads(workerThreads: 200, completionPortThreads: 200);

app.Run();
