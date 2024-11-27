
using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;
using Polly;

namespace CryptoAlertsBackend.Workers
{
    public class CleanerService(IServiceScopeFactory serviceScopeFactory, ILogger<CleanerService> logger) : BackgroundService
    {
        private readonly TimeSpan _delay = TimeSpan.FromMinutes(60); // Set calculation interval

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("CleanerService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();

                    // Resolve the DbContext and PriceService
                    var dbContext = scope.ServiceProvider.GetRequiredService<EndpointContext>();

                    // Define the cutoff date for deletion
                    var cutoffDate = DateTime.Now - TimeSpan.FromHours(26);

                    // Delete records older than the cutoff date
                    var deletedCount = await dbContext.PriceRecords
                        .Where(pr => pr.DateTime < cutoffDate)
                        .ExecuteDeleteAsync(stoppingToken); // Using EF Core ExecuteDeleteAsync for efficiency


                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("All intervals processed. Changes saved.");
                }
                catch (Exception ex)
                {
                    logger.LogInformation("An error occurred while calculating assets.");
                    logger.LogInformation(ex.StackTrace);
                    logger.LogInformation(ex.InnerException?.StackTrace);
                }

                await Task.Delay(_delay, stoppingToken);
            }

            logger.LogInformation("CleanerService is stopping.");
        }
    }
}
