using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoAlertsBackend.Models
{
    [Table("PriceRecords")]
    public class PriceRecord
    {
        [Key]
        public int Id { get; set; }
        public float Price { get; set; } = 0.0f;
        public DateTime DateTime { get; set; } = DateTime.Now;
        // Foreign Key
        public int AssetId { get; set; }

        // Navigation property back to Asset
        public Asset? Asset { get; set; }
    }
}
