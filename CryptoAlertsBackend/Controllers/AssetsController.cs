using Microsoft.AspNetCore.Mvc;
using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;
using static CryptoAlertsBackend.Models.DiscordUtils;

namespace CryptoAlertsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController(EndpointContext context, AssetService assetService) : Controller
    {
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_5M = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_5M");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_15M = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_15M");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_30M = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_30M");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_1H = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_1H");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_4H = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_4H");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_8H = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_8H");
        private static readonly float? MINIMUM_PRICE_CHANGE_TO_ALERT_24H = TryParseEnvironmentVariableOrNull("MINIMUM_PRICE_CHANGE_TO_ALERT_24H");

        // minutes: value_min
        private readonly Dictionary<int, float> ChangeTimeframes = new Dictionary<int, float>()
            {
                {5, MINIMUM_PRICE_CHANGE_TO_ALERT_5M ?? 3f},
                {15, MINIMUM_PRICE_CHANGE_TO_ALERT_15M ?? 5f},
                {30, MINIMUM_PRICE_CHANGE_TO_ALERT_30M ?? 6f},
                {60, MINIMUM_PRICE_CHANGE_TO_ALERT_1H ?? 7f},
                {240, MINIMUM_PRICE_CHANGE_TO_ALERT_4H ?? 10f},
                {480, MINIMUM_PRICE_CHANGE_TO_ALERT_8H ?? 15f},
                {1440, MINIMUM_PRICE_CHANGE_TO_ALERT_24H ?? 20f}
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
            try
            {
                var assetsFound = await context.Assets
                    .Where(asset => asset.Name == priceRecordCreateDto.AssetName).ToListAsync();

                if (assetsFound is not List<Asset> { Count: > 0 })
                    return NotFound(priceRecordCreateDto);

                Asset assetFound = assetsFound.First();

                _ = Task.Run(async () =>
                {
                    foreach (var (minutes, minChange) in ChangeTimeframes)
                    {
                        (PriceRecord historyPriceRecord, float change, Dictionary<string, bool> athatl)
                        =
                        await assetService.CheckIfPriceChangedAsync(assetFound, priceRecordCreateDto,
                            TimeSpan.FromMinutes(minutes));
                        float currentPrice = priceRecordCreateDto.Price;

                        if (currentPrice == historyPriceRecord.Price || historyPriceRecord.Price == 0 || change < minChange)
                            continue;

                        string baseNotification = $"{assetFound.Name}\n{historyPriceRecord.Price} => {currentPrice}$\n{historyPriceRecord.DateTime} | {DateTime.Now}";

                        Notification notificationToSend = new()
                        {
                            NotificationText = baseNotification,
                            NotificationType = "price_change",
                            Extra = new()
                            {
                                RatioIfHigherPrice = (double)change / minChange,
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

                    }

                    await assetService.SavePriceRecordToDatabase(priceRecordCreateDto, assetFound.Id);
                });
                return Ok(priceRecordCreateDto);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
