using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;

namespace CryptoAlertsBackend.Models
{
    public class AssetService(IServiceScopeFactory serviceScopeFactory)
    {
        //private static readonly SemaphoreSlim semaphore = new(48); // Limit to 48 concurrent tasks

        // Returns lastPrice and change % and athatl
        public (PriceRecord, float, Dictionary<string, bool>) CheckIfPriceChanged(
            List<PriceRecord> priceRecords,
            PriceRecordCreateDto priceRecordCreateDto,
            TimeSpan timeFrame,
            DateTime currentTime)
        {
            var targetTime = currentTime - timeFrame;

            // Fetch the closest record directly from the database
            var relevantRecords = priceRecords
                .OrderBy(pr => Math.Abs(EF.Functions.DateDiffSecond(targetTime, pr.DateTime))) // Sort by closest to targetTime
                .ToList();
            if (relevantRecords == null || relevantRecords.Count == 0)
                return (new PriceRecord(), 0.0f, []);

            PriceRecord closestRecord = relevantRecords.First();
            float historicPrice = closestRecord.Price;
            float priceChange = Math.Abs((priceRecordCreateDto.Price / historicPrice * 100) - 100);

            // Compute ATH/ATL using the fetched records
            Dictionary<string, bool> athatl = CheckIfPriceWasATHorATL(
                relevantRecords,
                priceRecordCreateDto.Price);

            return (closestRecord, float.Parse(priceChange.ToString("0.000")), athatl);
        }


        public Dictionary<string, bool> CheckIfPriceWasATHorATL(List<PriceRecord> allPriceRecords, float currentPrice)
        {
            float maxPrice = allPriceRecords.Select(pr => pr.Price).Max();
            float minPrice = allPriceRecords.Select(pr => pr.Price).Min();

            return new Dictionary<string, bool>
            {
                { "wasATH", currentPrice > maxPrice },
                { "wasATL", currentPrice < minPrice }
            };
        }
        public async Task SavePriceRecordToDatabase(float price, DateTime currentTime, int assetId)
        {
            PriceRecord priceRecord = new()
            {
                DateTime = currentTime,
                Price = price,
                AssetId = assetId
            };

            using var scope = serviceScopeFactory.CreateScope();
            // Resolve the DbContext
            var context = scope.ServiceProvider.GetRequiredService<EndpointContext>();
            context.PriceRecords.Add(priceRecord);
            await context.SaveChangesAsync();
        }

        // 1 minute, 5 minutes, 15 minutes, 30 minutes, 60 minutes, 240 minutes, 480 minutes, 1440 minutes
        public async Task<List<PriceRecord>> GetRelevantRecords(int assetId, int interval,
            DateTime currentTime, int timestampMinutesThreshold)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<EndpointContext>();
            var returnDict = new List<PriceRecord>();

            // Calculate the target time and bounds
            DateTime targetTime = currentTime.AddMinutes(-interval);
            DateTime lowerBound = targetTime.AddMinutes(-timestampMinutesThreshold);
            DateTime upperBound = targetTime.AddMinutes(timestampMinutesThreshold);

            // Query records within the threshold for this interval
            return await _context.PriceRecords
                .Include(pr => pr.Asset) // Remove this if Asset data is unnecessary
                .Where(x=>x.AssetId == assetId && x.DateTime >= lowerBound && x.DateTime <= upperBound)
                .OrderBy(x => x.DateTime)
                .AsNoTracking() // Improve read-only performance
                .ToListAsync();
        }
    }
}
