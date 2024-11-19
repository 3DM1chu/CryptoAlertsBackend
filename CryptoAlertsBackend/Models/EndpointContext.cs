using Microsoft.EntityFrameworkCore;

namespace CryptoAlertsBackend.Models
{
    public class EndpointContext(DbContextOptions<EndpointContext> options) : DbContext(options)
    {
        public DbSet<Endpoint> Endpoints { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<PriceRecord> PriceRecords { get; set; }
    }
}
