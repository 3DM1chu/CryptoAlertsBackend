namespace CryptoAlertsBackend.Models
{
    public class EndpointDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public List<AssetDto> Assets { get; set; } = [];
    }

    public class EndpointInitiateDto
    {
        public string Name { get; set; } = "";
    }

    public class AssetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<PriceRecordDto> PriceRecords { get; set; } = [];
    }

    public class AddAssetToEndpointDto
    {
        public string EndpointName { get; set; } = "";
        public string AssetName { get; set; } = "";
    }

    public class PriceRecordDto
    {
        public int Id { get; set; }
        public float Price { get; set; } = 0.0f;
        public DateTime DateTime { get; set; } = DateTime.Now;
    }

    public class PriceRecordCreateDto
    {
        public float Price { get; set; } = 0.0f;
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string AssetName { get; set; } = "";
        public string EndpointName { get; set; } = "";
    }
}
