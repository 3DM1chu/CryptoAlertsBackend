
using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CryptoAlertsBackend.Workers
{
    public class TestBgService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<TestBgService> _logger;
        private readonly TimeSpan _delay = TimeSpan.FromSeconds(5); // Set calculation interval

        public TestBgService(IServiceScopeFactory serviceScopeFactory, ILogger<TestBgService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AssetCalculationService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    // Resolve the DbContext and PriceService
                    var dbContext = scope.ServiceProvider.GetRequiredService<EndpointContext>();
                    var priceService = scope.ServiceProvider.GetRequiredService<AssetService>();

                    // Define parameters for the price check
                    int assetId = 1; // Example asset ID, replace as needed
                    var timeFrame = TimeSpan.FromHours(24); // Example time frame, e.g., 24 hours
                    float minPriceChangePercent = 2.0f; // Example minimum change
                    float currentPrice = 100.0f; // Example current price

                    // Initialize PriceService with token ID
                    var priceServiceInstance = new AssetService(dbContext);

                    // Perform price check
                    var priceChange = await priceServiceInstance.CheckIfPriceChangedAsync(
                        timeFrame,
                        minPriceChangePercent,
                        currentPrice,
                        assetId
                    );

                    // Log or process the price change result
                    if (priceChange.HasValue && priceChange.Value >= minPriceChangePercent)
                    {
                        _logger.LogInformation($"Price change detected: {priceChange}% for asset {assetId}");
                    }
                    else
                    {
                        _logger.LogInformation("No significant price change detected.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while calculating assets.");
                }

                await Task.Delay(_delay, stoppingToken);
            }

            _logger.LogInformation("AssetCalculationService is stopping.");
        }

        private async Task CalculateAssets(EndpointContext dbContext, CancellationToken stoppingToken)
        {
            // Sample calculation logic
            var endpoints = await dbContext.Endpoints.Include(e => e.Assets).ToListAsync(stoppingToken); // Replace with your entity

            foreach (var endpoint in endpoints)
            {
                var assets = endpoint.Assets;
                foreach (var asset in assets)
                {
                    asset.Name = CalculateNewPrice(asset);
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken); // Save changes to the database
            _logger.LogInformation("Assets recalculated successfully.");
        }

        private string CalculateNewPrice(Asset asset)
        {
            // Replace with actual price calculation logic
            return asset.Name + "a"; // Example: Increase price by 2%
        }
    }
}
