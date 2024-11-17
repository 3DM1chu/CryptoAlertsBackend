using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CryptoAlertsBackend.Models;
using Asset = CryptoAlertsBackend.Models.Asset;
using Endpoint = CryptoAlertsBackend.Models.Endpoint;

namespace CryptoAlertsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EndpointsController(EndpointContext context) : ControllerBase
    {

        // GET: api/Endpoints
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EndpointDto>>> GetEndpoints()
        {
            var endpoints = await context.Endpoints.Include(e => e.Assets).ToListAsync();
            return endpoints.Select(DTOMapper.ToEndpointDto).ToList();
        }

        // GET: api/Endpoints/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Endpoint>> GetEndpoint(int id)
        {
            var endpoint = await context.Endpoints.Include(e => e.Assets).FirstOrDefaultAsync(e => e.Id == id);

            if (endpoint == null)
            {
                return NotFound();
            }

            return endpoint;
        }

        // POST: api/Endpoints
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Endpoint>> CreateEndpoint(Endpoint endpoint)
        {
            context.Endpoints.Add(endpoint);
            await context.SaveChangesAsync();

            return endpoint;
        }

        // POST: api/Endpoints/AppendAsset
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("AppendAsset")]
        public async Task<ActionResult<Asset>> AddAssetToEndpoint(AddAssetToEndpointDto addAssetToEndpointDto)
        {
            var endpoint = await context.Endpoints
                .Include(e => e.Assets)
                .Where(e => e.Name == addAssetToEndpointDto.EndpointName)
                .FirstOrDefaultAsync();

            if (endpoint == null)
                return NotFound();

            var asset = endpoint.Assets.FirstOrDefault(asset => asset.Name == addAssetToEndpointDto.AssetName);
            if (asset == null)
            {
                asset = new Asset() { Name = addAssetToEndpointDto.AssetName };
                context.Assets.Add(asset);
                endpoint.Assets.Add(asset);
                await context.SaveChangesAsync();
            }
            else
            {
                return BadRequest();
            }

            return Ok();
        }

        // DELETE: api/Endpoint/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEndpoint(int id)
        {
            var endpoint = await context.Endpoints.FindAsync(id);
            if (endpoint == null)
            {
                return NotFound();
            }

            context.Endpoints.Remove(endpoint);
            await context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Endpoint/5/Asset
        [HttpDelete("{id}/Asset/{Asset_symbol}")]
        public async Task<IActionResult> DeleteEndpointAsset(int id, string Asset_symbol)
        {
            var endpoint = await context.Endpoints.FindAsync(id);
            if (endpoint == null)
            {
                return NotFound();
            }

            var Asset = await context.Assets.FirstOrDefaultAsync(t => t.Name == Asset_symbol);
            if (Asset == null)
            {
                return NotFound();
            }

            context.Assets.Remove(Asset);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool EndpointExists(int id)
        {
            return context.Endpoints.Any(e => e.Id == id);
        }
    }
}
