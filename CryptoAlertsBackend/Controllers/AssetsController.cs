using Microsoft.AspNetCore.Mvc;
using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using static CryptoAlertsBackend.Models.DiscordUtils;

namespace CryptoAlertsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController(EndpointContext context, AssetService assetService) : Controller
    {
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_5M = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_5M");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_15M = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_15M");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_30M = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_30M");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_1H = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_1H");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_4H = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_4H");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_8H = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_8H");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_24H = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_24H");

        // minutes: value_min
        private Dictionary<int, double> ChangeTimeframes = new Dictionary<int, double>()
            {
                {5, MINIMUM_PRICE_CHANGE_TO_ALERT_5M},
                {15, MINIMUM_PRICE_CHANGE_TO_ALERT_15M},
                {30, MINIMUM_PRICE_CHANGE_TO_ALERT_30M},
                {60, MINIMUM_PRICE_CHANGE_TO_ALERT_1H},
                {240, MINIMUM_PRICE_CHANGE_TO_ALERT_4H},
                {480, MINIMUM_PRICE_CHANGE_TO_ALERT_8H},
                {1440, MINIMUM_PRICE_CHANGE_TO_ALERT_24H}
            };

        [HttpPost("addPriceRecord")]
        public async Task<IActionResult> AddPriceRecordToAsset(PriceRecordCreateDto priceRecordCreateDto)
        {
            var assetFound = await context.Assets
                .Where(asset => asset.Name == priceRecordCreateDto.AssetName)
                .Include(ass => ass.PriceRecords)
                .Include(ass => ass.Endpoint).FirstAsync();

            if(assetFound is null)
                return NotFound(priceRecordCreateDto);

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

                    DiscordUtils.SendNotification(notificationToSend);
                }

                await assetService.SavePriceRecordToDatabase(priceRecordCreateDto, assetFound.Id);
            });
            return Ok(priceRecordCreateDto);
        }
    }
}
