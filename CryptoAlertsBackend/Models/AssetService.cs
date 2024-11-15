using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoAlertsBackend.Models
{
    public class AssetService(IServiceScopeFactory serviceScopeFactory)
    {
        public async Task<float?> CheckIfPriceChangedAsync(Asset asset, PriceRecordCreateDto priceRecordCreateDto, TimeSpan timeFrame, float minPriceChangePercent)
        {
            PriceRecord priceRecord = new()
            {
                DateTime = priceRecordCreateDto.DateTime,
                Price = priceRecordCreateDto.Price,
                AssetId = asset.Id
            };

            using var scope = serviceScopeFactory.CreateScope();

            // Resolve the DbContext
            var context = scope.ServiceProvider.GetRequiredService<EndpointContext>();

            // Calculate target time based on the given time frame
            var targetTime = DateTime.Now - timeFrame;

            var priceRecords = await context.PriceRecords
                .Include(pr => pr.Asset)
                .Where(pr => pr.AssetId == asset.Id)
                .ToListAsync();

            if(priceRecords.Count == 0)
            {
                // first price record
                context.PriceRecords.Add(priceRecord);
                await context.SaveChangesAsync();
                return null;
            }

            var filteredTimeSpanPriceRecords = priceRecords.Where(pr => pr.DateTime >= targetTime).ToList();
            PriceRecord closestRecord;

            if (filteredTimeSpanPriceRecords.Count == 0)
            {
                // If no records are found after the target time, get the closest record before targetTime
                closestRecord = priceRecords
                    .Where(pr => pr.DateTime <= targetTime) // Ensure you're looking only at records before or equal to targetTime
                    .OrderByDescending(pr => pr.DateTime)  // Get the latest record before targetTime
                    .First(); // Get the last one (most recent before targetTime)
            }
            else
            {
                // Find the closest price record after target time
                closestRecord = filteredTimeSpanPriceRecords
                    .OrderBy(pr => Math.Abs((pr.DateTime - targetTime).TotalSeconds)) // Find closest after targetTime
                    .First();
            }

            // Extract the historical price and date
            var historicPrice = closestRecord.Price;
            var historicDateTime = closestRecord.DateTime.AddHours(1); // Adding an hour as in the original function

            // Calculate price change percentage
            var priceChange = Math.Abs((priceRecord.Price / historicPrice * 100) - 100);

            Dictionary<string, bool> athatl = checkIfPriceWasATHorATL(timeFrame, priceRecord.Price, asset.Id, context);

            context.PriceRecords.Add(priceRecord);
            await context.SaveChangesAsync();


            Console.WriteLine("Price changed " + float.Parse(priceChange.ToString("0.000")).ToString());

            // Optionally format the result to three decimal places
            return float.Parse(priceChange.ToString("0.000"));
        }

        public Dictionary<string, bool> checkIfPriceWasATHorATL(TimeSpan timeFrame, float currentPrice,
            int assetId, EndpointContext context)
        {
            var targetTime = DateTime.Now - timeFrame;
            var allPriceRecords = context.PriceRecords
                .Where(pr => pr.AssetId == assetId && pr.DateTime >= targetTime)
                .ToList();

            // If no records are found, return a default response
            if (allPriceRecords.Count == 0)
            {
                return new Dictionary<string, bool>
                {
                    { "wasATH", false },
                    { "wasATL", false }
                };
            }

            float maxPrice = allPriceRecords.Select(pr => pr.Price).Max();
            float minPrice = allPriceRecords.Select(pr => pr.Price).Min();

            return new Dictionary<string, bool>
            {
                { "wasATH", currentPrice > maxPrice },
                { "wasATL", currentPrice < minPrice }
            };
        }

    }
}
