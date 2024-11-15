using Microsoft.AspNetCore.Mvc;
using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;


namespace CryptoAlertsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : Controller
    {
        private readonly EndpointContext _context;
        private readonly AssetService assetService;
        private readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_5M = Env.GetDouble("ConnectionStrings__DBCon");
        private readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_15M = Env.GetDouble("ConnectionStrings__DBCon");
        private readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_30M = Env.GetDouble("ConnectionStrings__DBCon");
        private readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_1H = Env.GetDouble("ConnectionStrings__DBCon");
        private readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_4H = Env.GetDouble("ConnectionStrings__DBCon");
        private readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_8H = Env.GetDouble("ConnectionStrings__DBCon");
        private readonly double MINIMUM_PRICE_CHANGE_TO_ALERT_24H = Env.GetDouble("ConnectionStrings__DBCon");

        public AssetsController(EndpointContext context, AssetService assetService)
        {
            _context = context;
            this.assetService = assetService;
        }

        [HttpPost("addPriceRecord")]
        public async Task<IActionResult> AddPriceRecordToAsset(PriceRecordCreateDto priceRecordCreateDto)
        {
            var assetFound = await _context.Assets
                .Where(asset => asset.Name == priceRecordCreateDto.AssetName)
                .Include(ass => ass.Endpoint).FirstAsync();

            if(assetFound == null)
            {
                return NotFound(priceRecordCreateDto);
            }
            _ = Task.Run(async () =>
            {
                await assetService.CheckIfPriceChangedAsync(assetFound, priceRecordCreateDto, TimeSpan.FromMinutes(5), (float)MINIMUM_PRICE_CHANGE_TO_ALERT_5M);
                await assetService.CheckIfPriceChangedAsync(assetFound, priceRecordCreateDto, TimeSpan.FromMinutes(15), (float)MINIMUM_PRICE_CHANGE_TO_ALERT_15M);
                await assetService.CheckIfPriceChangedAsync(assetFound, priceRecordCreateDto, TimeSpan.FromMinutes(30), (float)MINIMUM_PRICE_CHANGE_TO_ALERT_30M);
                await assetService.CheckIfPriceChangedAsync(assetFound, priceRecordCreateDto, TimeSpan.FromHours(1), (float)MINIMUM_PRICE_CHANGE_TO_ALERT_1H);
                await assetService.CheckIfPriceChangedAsync(assetFound, priceRecordCreateDto, TimeSpan.FromHours(4), (float)MINIMUM_PRICE_CHANGE_TO_ALERT_4H);
                await assetService.CheckIfPriceChangedAsync(assetFound, priceRecordCreateDto, TimeSpan.FromHours(8), (float)MINIMUM_PRICE_CHANGE_TO_ALERT_8H);
                await assetService.CheckIfPriceChangedAsync(assetFound, priceRecordCreateDto, TimeSpan.FromHours(24), (float)MINIMUM_PRICE_CHANGE_TO_ALERT_24H);
            });
            return Ok(priceRecordCreateDto);
        }
    }
}
