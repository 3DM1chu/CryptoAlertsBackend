using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CryptoAlertsBackend.Models;
using Asset = CryptoAlertsBackend.Models.Asset;
using Endpoint = CryptoAlertsBackend.Models.Endpoint;

namespace CryptoAlertsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EndpointsController : ControllerBase
    {
        private readonly EndpointContext _context;

        public EndpointsController(EndpointContext context)
        {
            _context = context;
        }

        // GET: api/Endpoints
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EndpointDto>>> GetEndpoints()
        {
            var endpoints = await _context.Endpoints.Include(e => e.Assets).ToListAsync();
            return endpoints.Select(DTOMapper.ToEndpointDto).ToList();
        }

        // GET: api/Endpoints/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Endpoint>> GetEndpoint(int id)
        {
            var endpoint = await _context.Endpoints.Include(e => e.Assets).FirstOrDefaultAsync(e => e.Id == id);

            if (endpoint == null)
            {
                return NotFound();
            }

            return endpoint;
        }

        // PUT: api/Endpoints/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEndpoint(int id, Asset endpoint)
        {
            if (id != endpoint.Id)
            {
                return BadRequest();
            }

            _context.Entry(endpoint).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EndpointExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Endpoints
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Endpoint>> CreateEndpoint(Endpoint endpoint)
        {
            _context.Endpoints.Add(endpoint);
            await _context.SaveChangesAsync();

            return endpoint;
        }

        // POST: api/Endpoints/5/Asset
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{id}/Asset")]
        public async Task<ActionResult<Asset>> AddAssetToEndpoint(int id, Asset _Asset)
        {
            var endpoint = await _context.Endpoints.Include(e => e.Assets).FirstOrDefaultAsync(e => e.Id == id);
            if (endpoint == null)
            {
                return NotFound();
            }

            var Asset = endpoint.Assets.FirstOrDefault(e => e.Name == _Asset.Name);
            if (Asset == null)
            {
                Asset = new Asset() { Name = _Asset.Name };
                _context.Assets.Add(Asset);
                endpoint.Assets.Add(Asset);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // DELETE: api/Endpoint/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEndpoint(int id)
        {
            var endpoint = await _context.Endpoints.FindAsync(id);
            if (endpoint == null)
            {
                return NotFound();
            }

            _context.Endpoints.Remove(endpoint);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Endpoint/5/Asset
        [HttpDelete("{id}/Asset/{Asset_symbol}")]
        public async Task<IActionResult> DeleteEndpointAsset(int id, string Asset_symbol)
        {
            var endpoint = await _context.Endpoints.FindAsync(id);
            if (endpoint == null)
            {
                return NotFound();
            }

            var Asset = await _context.Assets.FirstOrDefaultAsync(t => t.Name == Asset_symbol);
            if (Asset == null)
            {
                return NotFound();
            }

            _context.Assets.Remove(Asset);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EndpointExists(int id)
        {
            return _context.Endpoints.Any(e => e.Id == id);
        }
    }
}
