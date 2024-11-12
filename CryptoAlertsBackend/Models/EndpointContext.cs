using Microsoft.EntityFrameworkCore;
using System.Xml;

namespace CryptoAlertsBackend.Models
{
    public class EndpointContext: DbContext
    {
        public EndpointContext(DbContextOptions<EndpointContext> options) : base(options) { }
        public DbSet<Endpoint> Endpoints { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<PriceRecord> PriceRecords { get; set; }
    }
}
