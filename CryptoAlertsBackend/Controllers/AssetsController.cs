using Microsoft.AspNetCore.Mvc;
using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using System.Text;
using Azure.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace CryptoAlertsBackend.Controllers
{
    public class Notification
    {
        public string notificationText { get; set; } = "";
        public string notificationType { get; set; } = "";
        public ExtraData extra { get; set; } = new() { ratioIfHigherPrice = 0.0, wentUp = false };
    }

    public class ExtraData
    {
        public double ratioIfHigherPrice { get; set; } = 0.0;
        public bool wentUp { get; set; } = false;
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : Controller
    {
        private readonly EndpointContext _context;
        private readonly AssetService assetService;
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_5M = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_5M");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_15M = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_15M");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_30M = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_30M");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_1H = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_1H");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_4H = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_4H");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_8H = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_8H");
        private static readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_24H = Env.GetDouble("MINIMUM_PRICE_CHANGE_TO_ALERT_24H");

        private static readonly string DISCORD_WEBHOOK_NORMAL_ALERT_URL = Env.GetString("DISCORD_WEBHOOK_NORMAL_ALERT_URL");
        private static readonly string DISCORD_WEBHOOK_2X_RATIO_URL = Env.GetString("DISCORD_WEBHOOK_2X_RATIO_URL");
        private static readonly string DISCORD_WEBHOOK_3X_RATIO_URL = Env.GetString("DISCORD_WEBHOOK_3X_RATIO_URL");

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

        public AssetsController(EndpointContext context, AssetService assetService)
        {
            _context = context;
            this.assetService = assetService;
        }

        [HttpPost("addPriceRecord")]
        public async Task<IActionResult> AddPriceRecordToAsset(PriceRecordCreateDto priceRecordCreateDto)
        {
            var assetFound = await _context.Assets
                .Where(asset => asset.Name == priceRecordCreateDto.AssetName)
                .Include(ass => ass.PriceRecords)
                .Include(ass => ass.Endpoint).FirstAsync();

            if(assetFound == null)
            {
                return NotFound(priceRecordCreateDto);
            }

            _ = Task.Run(async () =>
            {

                foreach (var (minutes, minChange) in ChangeTimeframes)
                {
                    (PriceRecord historyPriceRecord, float change, Dictionary<string, bool> athatl) = await assetService.CheckIfPriceChangedAsync(assetFound, priceRecordCreateDto,
                        TimeSpan.FromMinutes(minutes), (float)minChange);
                    float currentPrice = priceRecordCreateDto.Price;
                    string baseNotification = $"{assetFound.Name}\n{historyPriceRecord.Price} => {currentPrice}$\n{historyPriceRecord.DateTime} | {DateTime.Now}";

                    Notification notificationToSend = new()
                    {
                        notificationText = baseNotification,
                        notificationType = "price_change",
                        extra = new()
                        {
                            ratioIfHigherPrice = (double)change / minChange,
                            wentUp = true
                        }
                    };

                    if (currentPrice > historyPriceRecord.Price && athatl["wasATH"])
                    {
                        notificationToSend.notificationText = "```" + $"\"{baseNotification}\nATH in {minutes} minutes\n📗{change}%" + "```";
                    }
                    else if (currentPrice < historyPriceRecord.Price && athatl["wasATL"])
                    {
                        notificationToSend.notificationText = "```" + $"{baseNotification}\nATL in {minutes}  minutes\n📗 {change}%" + "```";
                        notificationToSend.extra.wentUp = false;
                    }
                    SendNotification(notificationToSend);

                }
            });
            return Ok(priceRecordCreateDto);
        }

        private void SendNotification(Notification notification)
        {
            string formatToAdd = "";
            HttpClient client = new();
            if (notification.notificationType == "price_change")
            {
                if (notification.extra.wentUp)
                    formatToAdd = "fix\n";
                else
                    formatToAdd = "\n";

                string baseMessage = $"{formatToAdd}{notification.notificationText}";
                string jsonPayload = $"{{\"content\":\"```{baseMessage}```\"}}";

                // Send normal alert
                HttpRequestMessage request = new(HttpMethod.Post, DISCORD_WEBHOOK_NORMAL_ALERT_URL)
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };
                var resp = client.Send(request);

                // Check if ratio conditions are met and send accordingly
                if (2.0 <= notification.extra.ratioIfHigherPrice && notification.extra.ratioIfHigherPrice < 3.0)
                {
                    // Send 2X ratio alert
                    HttpRequestMessage request2 = new(HttpMethod.Post, DISCORD_WEBHOOK_2X_RATIO_URL)
                    {
                        Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                    };
                    client.Send(request2);
                }
                else if (notification.extra.ratioIfHigherPrice >= 3)
                {
                    // Send 3X ratio alert
                    HttpRequestMessage request2 = new(HttpMethod.Post, DISCORD_WEBHOOK_3X_RATIO_URL)
                    {
                        Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                    };
                    client.Send(request2);
                }
            }
            else if (notification.notificationType == "price_level")
            {
                // TODO
            }
            else
            {
                Console.WriteLine($"Dont know type: {notification.notificationType}");
                return;
            }

        }
    }
}
