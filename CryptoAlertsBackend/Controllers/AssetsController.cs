using Microsoft.AspNetCore.Mvc;
using CryptoAlertsBackend.Models;
using Microsoft.EntityFrameworkCore;
using CryptoAlertsBackend.Migrations;
using NuGet.ContentModel;
using Microsoft.EntityFrameworkCore.Internal;

namespace CryptoAlertsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : Controller
    {
        private readonly EndpointContext _context;
        private readonly AssetService assetService;

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
                await assetService.CheckIfPriceChangedAsync(assetFound, priceRecordCreateDto, TimeSpan.FromMinutes(30), 2.0f);
            });
            return Ok(priceRecordCreateDto);
        }
    }
}
