namespace CryptoAlertsBackend.Models
{
    public class AssetService(IServiceScopeFactory serviceScopeFactory)
    {
        // Returns lastPrice and change % and athatl
        public async Task<(PriceRecord, float, Dictionary<string, bool>)> CheckIfPriceChangedAsync(Asset asset, PriceRecordCreateDto priceRecordCreateDto,
            TimeSpan timeFrame)
        {
            if(asset.PriceRecords.Count == 0)
                return (new PriceRecord(), 0.0f, []);

            PriceRecord lastPriceRecord = asset.PriceRecords.Last();

            // Calculate target time based on the given time frame
            var targetTime = DateTime.Now - timeFrame;

            if(asset.PriceRecords.Count == 0)
                return (new PriceRecord(), 0.0f, []);

            var filteredTimeSpanPriceRecords = asset.PriceRecords.Where(pr => pr.DateTime >= targetTime).ToList();
            PriceRecord closestRecord;

            if (filteredTimeSpanPriceRecords.Count == 0)
            {
                // If no records are found after the target time, get the closest record before targetTime
                closestRecord = asset.PriceRecords
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
            var historicDateTime = closestRecord.DateTime; // Adding an hour as in the original function

            // Calculate price change percentage
            var priceChange = Math.Abs((priceRecordCreateDto.Price / historicPrice * 100) - 100);
            Dictionary<string, bool> athatl = await CheckIfPriceWasATHorATL(timeFrame, asset, priceRecordCreateDto.Price);

            return (lastPriceRecord, float.Parse(priceChange.ToString("0.000")), athatl);
        }

        public async Task<Dictionary<string, bool>> CheckIfPriceWasATHorATL(TimeSpan timeFrame, Asset asset, float currentPrice)
        {
            var targetTime = DateTime.Now - timeFrame;
            var allPriceRecords = asset.PriceRecords
                .Where(pr => pr.DateTime >= targetTime)
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
