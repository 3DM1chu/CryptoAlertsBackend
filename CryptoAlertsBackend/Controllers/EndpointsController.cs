using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CryptoAlertsBackend.Models;
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
        public async Task<ActionResult<IEnumerable<Endpoint>>> GetPriceRecords()
        {
            return await _context.Endpoints.ToListAsync();
        }

        // GET: api/Endpoints/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Endpoint>> GetEndpoint(int id)
        {
            var endpoint = await _context.Endpoints.FindAsync(id);

            if (endpoint == null)
            {
                return NotFound();
            }

            return endpoint;
        }

        // PUT: api/Endpoint/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEndpoint(int id, Endpoint endpoint)
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

        // POST: api/Endpoint
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Endpoint>> PostPriceRecord(Endpoint endpoint)
        {
            _context.Endpoints.Add(endpoint);
            await _context.SaveChangesAsync();

            return endpoint;
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

        private bool EndpointExists(int id)
        {
            return _context.Endpoints.Any(e => e.Id == id);
        }
    }
}
