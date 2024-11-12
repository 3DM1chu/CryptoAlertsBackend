namespace CryptoAlertsBackend.Models
{
    // Endpoint DTO
    public class EndpointDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public List<AssetDto> Assets { get; set; } = [];
    }

    // Asset DTO
    public class AssetDto
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = "";
    }
}
