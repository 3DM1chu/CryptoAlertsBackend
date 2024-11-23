using Microsoft.AspNetCore.Mvc;
using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;
using static CryptoAlertsBackend.Models.DiscordUtils;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoAlertsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController(EndpointContext context, AssetService assetService) : Controller
    {
        private static readonly int? DELAY_BETWEEN_CHECKS_1M = (int?)TryParseEnvironmentVariableOrNull("DELAY_BETWEEN_CHECKS_1M");
        private static readonly int? DELAY_BETWEEN_CHECKS_5M = (int?)TryParseEnvironmentVariableOrNull("DELAY_BETWEEN_CHECKS_5M");
        private static readonly int? DELAY_BETWEEN_CHECKS_15M = (int?)TryParseEnvironmentVariableOrNull("DELAY_BETWEEN_CHECKS_15M");
        private static readonly int? DELAY_BETWEEN_CHECKS_30M = (int?)TryParseEnvironmentVariableOrNull("DELAY_BETWEEN_CHECKS_30M");
        private static readonly int? DELAY_BETWEEN_CHECKS_1H = (int?)TryParseEnvironmentVariableOrNull("DELAY_BETWEEN_CHECKS_1H");
        private static readonly int? DELAY_BETWEEN_CHECKS_4H = (int?)TryParseEnvironmentVariableOrNull("DELAY_BETWEEN_CHECKS_4H");
        private static readonly int? DELAY_BETWEEN_CHECKS_8H = (int?)TryParseEnvironmentVariableOrNull("DELAY_BETWEEN_CHECKS_8H");
        private static readonly int? DELAY_BETWEEN_CHECKS_24H = (int?)TryParseEnvironmentVariableOrNull("DELAY_BETWEEN_CHECKS_24H");

        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_1M = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_1M");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_5M = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_5M");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_15M = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_15M");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_30M = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_30M");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_1H = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_1H");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_4H = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_4H");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_8H = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_8H");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_24H = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_24H");

        class Checks(int secs, float minChange)
        {
            public int SecondsOfDelay { get; set; } = secs;
            public float MinimumPriceChange { get; set; } = minChange;
        }

        // minutes: value_min
        private readonly Dictionary<int, Checks> ChangeTimeframes = new()
            {
                {1, new Checks(DELAY_BETWEEN_CHECKS_1M ?? 1, MINIMUM_PRICE_CHANGE_TO_ALERT_1M ?? 1f) },
                {5, new Checks(DELAY_BETWEEN_CHECKS_5M ?? 1, MINIMUM_PRICE_CHANGE_TO_ALERT_5M ?? 3f) },
                {15, new Checks(DELAY_BETWEEN_CHECKS_15M ?? 1, MINIMUM_PRICE_CHANGE_TO_ALERT_15M ?? 5f) },
                {30, new Checks(DELAY_BETWEEN_CHECKS_30M ?? 1, MINIMUM_PRICE_CHANGE_TO_ALERT_30M ?? 6f) },
                {60, new Checks(DELAY_BETWEEN_CHECKS_1H ?? 1, MINIMUM_PRICE_CHANGE_TO_ALERT_1H ?? 7f) },
                {240, new Checks(DELAY_BETWEEN_CHECKS_4H ?? 1, MINIMUM_PRICE_CHANGE_TO_ALERT_4H ?? 10f) },
                {480, new Checks(DELAY_BETWEEN_CHECKS_8H ?? 1, MINIMUM_PRICE_CHANGE_TO_ALERT_8H ?? 15f) },
                {1440, new Checks(DELAY_BETWEEN_CHECKS_24H ?? 1, MINIMUM_PRICE_CHANGE_TO_ALERT_24H ?? 20f) }
            };


        private static float? TryParseEnvironmentVariableOrNull(string key)
        {
            string? value = Environment.GetEnvironmentVariable(key);
            if (value == null)
            {
                return null;
            }
            if (float.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }


        [HttpPost("addPriceRecord")]
        public async Task<IActionResult> AddPriceRecordToAsset(PriceRecordCreateDto priceRecordCreateDto)
        {
            var assetsFound = await context.Assets
                .Where(asset => asset.Name == priceRecordCreateDto.AssetName).ToListAsync();

            if (assetsFound is not List<Asset> { Count: > 0 })
                return NotFound(priceRecordCreateDto);

            Asset assetFound = assetsFound.First();

            _ = Task.Run(async () =>
            {
                await Parallel.ForEachAsync(ChangeTimeframes, async (changeChecksTimeframe, cancellationToken) =>
                {
                    var (minutes, check) = changeChecksTimeframe;
                    try
                    {
                        assetFound.LastTimeCheckedPrices.TryGetValue(minutes, out DateTime lastTimeChecked);
                        if(DateTime.Now - lastTimeChecked < TimeSpan.FromMilliseconds(check.SecondsOfDelay * 1000))
                        {
                            // Not "allowed" to check yet, since we really dont need to check for example 24h price every second
                            return;
                        }
                        else
                        {
                            assetFound.LastTimeCheckedPrices[minutes] = DateTime.Now;
                        }
                    }
                    catch (Exception)
                    {
                        // Not checked yet
                        // continue
                    }

                    (PriceRecord historyPriceRecord, float change, Dictionary<string, bool> athatl) =
                        await assetService.CheckIfPriceChangedAsync(assetFound, priceRecordCreateDto,
                            TimeSpan.FromMinutes(minutes));

                    float currentPrice = priceRecordCreateDto.Price;

                    if (currentPrice == historyPriceRecord.Price || historyPriceRecord.Price == 0 || change < check.MinimumPriceChange)
                        return;

                    string baseNotification = $"{assetFound.Name}\n{historyPriceRecord.Price} => {currentPrice}$\n{historyPriceRecord.DateTime} | {DateTime.Now}";

                    Notification notificationToSend = new()
                    {
                        NotificationText = baseNotification,
                        NotificationType = "price_change",
                        Extra = new()
                        {
                            RatioIfHigherPrice = (double)change / check.MinimumPriceChange,
                            WentUp = true
                        }
                    };

                    if (currentPrice > historyPriceRecord.Price && athatl["wasATH"])
                    {
                        notificationToSend.NotificationText = $"{baseNotification}\nATH in {minutes} minutes\n📗{change}%";
                    }
                    else if (currentPrice < historyPriceRecord.Price && athatl["wasATL"])
                    {
                        notificationToSend.NotificationText = $"{baseNotification}\nATL in {minutes} minutes\n📗 {change}%";
                        notificationToSend.Extra.WentUp = false;
                    }

                    if (athatl["wasATL"] || athatl["wasATH"])
                    {
                        DiscordUtils.SendNotification(notificationToSend);
                    }
                });

                await assetService.SavePriceRecordToDatabase(priceRecordCreateDto, assetFound.Id);
            });

            return Ok(priceRecordCreateDto);
        }
    }
}
