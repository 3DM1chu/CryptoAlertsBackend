using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoAlertsBackend.Models
{
    [Table("Endpoints")]
    public class Endpoint
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
    }
}
