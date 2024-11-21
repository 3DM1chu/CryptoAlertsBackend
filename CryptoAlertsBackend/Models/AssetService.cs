using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;

namespace CryptoAlertsBackend.Models
{
    public class AssetService(IServiceScopeFactory serviceScopeFactory)
    {
        private static readonly SemaphoreSlim semaphore = new(16); // Limit to 4 concurrent tasks

        // Returns lastPrice and change % and athatl
        public async Task<(PriceRecord, float, Dictionary<string, bool>)> CheckIfPriceChangedAsync(
            Asset asset,
            PriceRecordCreateDto priceRecordCreateDto,
            TimeSpan timeFrame)
        {
            await semaphore.WaitAsync();

            try
            {
                // Calculate target time based on the given time frame
                var targetTime = DateTime.Now - timeFrame;

                using var scope = serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<EndpointContext>();

                // Fetch the closest record directly from the database
                var closestRecord = await context.PriceRecords
                    .AsNoTracking()
                    .Where(pr => pr.AssetId == asset.Id && pr.DateTime >= targetTime) // Filter to relevant records
                    .OrderBy(pr => Math.Abs(EF.Functions.DateDiffSecond(targetTime, pr.DateTime))) // Sort by closest to targetTime
                    .Select(pr => new PriceRecord { Price = pr.Price, DateTime = pr.DateTime }) // Fetch full PriceRecord
                    .FirstOrDefaultAsync(); // Get the closest record

                if (closestRecord == null)
                    return (new PriceRecord(), 0.0f, []);

                // Extract the historical price
                var historicPrice = closestRecord.Price;

                // Calculate price change percentage
                var priceChange = Math.Abs((priceRecordCreateDto.Price / historicPrice * 100) - 100);

                // Fetch relevant records for ATH/ATL check, minimizing data
                var relevantRecords = await context.PriceRecords
                    .AsNoTracking()
                    .Where(pr => pr.AssetId == asset.Id && pr.DateTime >= targetTime)
                    .Select(pr => pr.Price) // Fetch only prices
                    .ToListAsync();

                // Compute ATH/ATL using the fetched records
                Dictionary<string, bool> athatl = CheckIfPriceWasATHorATL(
                    timeFrame,
                    relevantRecords,
                    priceRecordCreateDto.Price);

                return (closestRecord, float.Parse(priceChange.ToString("0.000")), athatl);
            }
            finally
            {
                semaphore.Release();
            }
        }


        public Dictionary<string, bool> CheckIfPriceWasATHorATL(TimeSpan timeFrame, List<float> allPriceRecords, float currentPrice)
        {
            float maxPrice = allPriceRecords.Max();
            float minPrice = allPriceRecords.Min();

            return new Dictionary<string, bool>
            {
                { "wasATH", currentPrice > maxPrice },
                { "wasATL", currentPrice < minPrice }
            };
        }
        public async Task SavePriceRecordToDatabase(PriceRecordCreateDto priceRecordCreateDto, int assetId)
        {
            PriceRecord priceRecord = new()
            {
                DateTime = priceRecordCreateDto.DateTime,
                Price = priceRecordCreateDto.Price,
                AssetId = assetId
            };

            using var scope = serviceScopeFactory.CreateScope();
            // Resolve the DbContext
            var context = scope.ServiceProvider.GetRequiredService<EndpointContext>();
            context.PriceRecords.Add(priceRecord);
            await context.SaveChangesAsync();
        }
    }
}
