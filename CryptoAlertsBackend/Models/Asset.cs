using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoAlertsBackend.Models
{
    [Table("Assets")]
    public class Asset
    {
        [Key]
        public int Id { get; set; }
        public string Symbol { get; set; } = "";
        // Foreign Key
        public int EndpointId { get; set; }

        // Navigation property back to Endpoint
        public Endpoint? Endpoint { get; set; }

    }
}
