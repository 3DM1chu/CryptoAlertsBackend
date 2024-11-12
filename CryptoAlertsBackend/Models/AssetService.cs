using Microsoft.EntityFrameworkCore;
using System;

namespace CryptoAlertsBackend.Models
{
    public class AssetService(EndpointContext context)
    {
        public async Task<float?> CheckIfPriceChangedAsync(TimeSpan timeFrame, float minPriceChangePercent, float currentPrice, int assetId)
        {
            // Calculate target time based on the given time frame
            var targetTime = DateTime.Now - timeFrame;

            // Get the price records from the database, filtering by assetId and targetTime
            var priceRecords = await context.PriceRecords
                .Where(pr => pr.AssetId == assetId && pr.DateTime >= targetTime)
                .ToListAsync(); // Fetch the data into memory first

            // Find the closest price record to the target time
            var closestRecord = priceRecords
                .OrderBy(pr => Math.Abs((pr.DateTime - targetTime).TotalSeconds)) // Perform the calculation client-side
                .Select(pr => new { pr.Price, pr.DateTime })
                .FirstOrDefault();

            // Check if a record was found
            if (closestRecord == null)
            {
                return null;
            }

            // Extract the historical price and date
            var historicPrice = closestRecord.Price;
            var historicDateTime = closestRecord.DateTime.AddHours(1); // Adding an hour as in the original function

            // Calculate price change percentage
            var priceChange = Math.Abs((currentPrice / historicPrice * 100) - 100);

            // Optionally format the result to three decimal places
            return float.Parse(priceChange.ToString("0.000"));
        }

    }
}
