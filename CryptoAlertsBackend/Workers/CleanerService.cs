
using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoAlertsBackend.Workers
{
    public class CleanerService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CleanerService> _logger;
        private readonly TimeSpan _delay = TimeSpan.FromMinutes(60); // Set calculation interval

        public CleanerService(IServiceScopeFactory serviceScopeFactory, ILogger<CleanerService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CleanerService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    // Resolve the DbContext and PriceService
                    var dbContext = scope.ServiceProvider.GetRequiredService<EndpointContext>();

                    // Define the cutoff date for deletion
                    var cutoffDate = DateTime.Now - TimeSpan.FromHours(26);

                    // Delete records older than the cutoff date
                    var deletedCount = await dbContext.PriceRecords
                        .Where(pr => pr.DateTime < cutoffDate)
                        .ExecuteDeleteAsync(stoppingToken); // Using EF Core ExecuteDeleteAsync for efficiency

                    await Task.Delay(_delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while calculating assets.");
                }

                await Task.Delay(_delay, stoppingToken);
            }

            _logger.LogInformation("CleanerService is stopping.");
        }
    }
}
