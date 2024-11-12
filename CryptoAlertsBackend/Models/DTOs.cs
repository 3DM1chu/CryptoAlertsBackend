namespace CryptoAlertsBackend.Models
{
    public class EndpointDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public List<AssetDto> Assets { get; set; } = [];
    }

    public class AssetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class PriceRecordCreateDto
    {
        public float Price { get; set; } = 0.0f;
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string AssetName { get; set; } = "";
        public string EndpointName { get; set; } = "";
    }
}
