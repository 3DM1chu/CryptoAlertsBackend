namespace CryptoAlertsBackend.Models
{
    public static class DTOMapper
    {
        public static EndpointDto ToEndpointDto(this Endpoint endpoint)
        {
            return new EndpointDto()
            {
                Id = endpoint.Id,
                Name = endpoint.Name,
                Assets = endpoint.Assets.Select(ToAssetDto).ToList()
            };
        }
        public static AssetDto ToAssetDto(this Asset Asset)
        {
            return new AssetDto()
            {
                Id = Asset.Id,
                Name = Asset.Name,
            };
        }
        public static PriceRecordDto ToPriceRecordDto(this PriceRecord priceRecord)
        {
            return new PriceRecordDto()
            {
                DateTime = priceRecord.DateTime,
                Price = priceRecord.Price,
            };
        }
    }
}
