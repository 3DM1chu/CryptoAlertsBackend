using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoAlertsBackend.Models
{
    [Table("Assets")]
    public class Asset
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<PriceRecord> PriceRecords { get; set; } = [];
        // Foreign Key
        public int EndpointId { get; set; }

        // Navigation property back to Endpoint
        public Endpoint? Endpoint { get; set; }


        /*
         * int -> minutes of MINIMUM_PRICE_CHANGE_TO_ALERT etc
         * It is used to check 
         * 
         * 1 min: 1s ago
         * 5 mins: 10s ago
         * 15 mins: 30s ago
         * 30 mins: 60s ago
         * etc
         */
        [NotMapped]
        public Dictionary<int, DateTime> LastTimeCheckedPrices { get; set; } = [];
    }
}
