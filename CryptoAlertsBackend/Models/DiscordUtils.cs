using DotNetEnv;
using JNogueira.Discord.Webhook.Client;

namespace CryptoAlertsBackend.Models
{
    public class DiscordUtils
    {
        public class Notification
        {
            public string NotificationText { get; set; } = "";
            public string NotificationType { get; set; } = "";
            public ExtraData Extra { get; set; } = new() { RatioIfHigherPrice = 0.0, WentUp = false };
        }

        public class ExtraData
        {
            public double RatioIfHigherPrice { get; set; } = 0.0;
            public bool WentUp { get; set; } = false;
        }

        private static readonly string DISCORD_WEBHOOK_NORMAL_ALERT_URL = Env.GetString("DISCORD_WEBHOOK_NORMAL_ALERT_URL");
        private static readonly string DISCORD_WEBHOOK_2X_RATIO_URL = Env.GetString("DISCORD_WEBHOOK_2X_RATIO_URL");
        private static readonly string DISCORD_WEBHOOK_3X_RATIO_URL = Env.GetString("DISCORD_WEBHOOK_3X_RATIO_URL");

        public static void SendNotification(Notification notification)
        {
            switch (notification.NotificationType)
            {
                case "price_change":
                    string formatToAdd = notification.Extra.WentUp ? "fix\n" : "\n";
                    string urlToSend = notification.Extra.RatioIfHigherPrice switch
                    {
                        <= 2.0 => DISCORD_WEBHOOK_NORMAL_ALERT_URL,
                        > 2.0 and < 3.0 => DISCORD_WEBHOOK_2X_RATIO_URL,
                        >= 3.0 => DISCORD_WEBHOOK_3X_RATIO_URL,
                        _ => throw new NotImplementedException()
                    };
                    new DiscordWebhookClient(urlToSend)
                        .SendToDiscord(new DiscordMessage($"```{formatToAdd}{notification.NotificationText}```"));
                    break;
                // TODO other cases like volume fe
                default:
                    break;
            }
        }
    }
}
