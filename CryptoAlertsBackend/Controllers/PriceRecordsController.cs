using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CryptoAlertsBackend.Models;

namespace CryptoAlertsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceRecordsController : ControllerBase
    {
        private readonly EndpointContext _context;

        public PriceRecordsController(EndpointContext context)
        {
            _context = context;
        }

        // GET: api/PriceRecords
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PriceRecord>>> GetPriceRecords()
        {
            return await _context.PriceRecords.ToListAsync();
        }

        // GET: api/PriceRecords/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PriceRecord>> GetPriceRecord(int id)
        {
            var priceRecord = await _context.PriceRecords.FindAsync(id);

            if (priceRecord == null)
            {
                return NotFound();
            }

            return priceRecord;
        }

        // PUT: api/PriceRecords/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPriceRecord(int id, PriceRecord priceRecord)
        {
            if (id != priceRecord.Id)
            {
                return BadRequest();
            }

            _context.Entry(priceRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PriceRecordExists(id))
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

        // POST: api/PriceRecords
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PriceRecord>> PostPriceRecord(PriceRecord priceRecord)
        {
            _context.PriceRecords.Add(priceRecord);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPriceRecord", new { id = priceRecord.Id }, priceRecord);
        }

        // DELETE: api/PriceRecords/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePriceRecord(int id)
        {
            var priceRecord = await _context.PriceRecords.FindAsync(id);
            if (priceRecord == null)
            {
                return NotFound();
            }

            _context.PriceRecords.Remove(priceRecord);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PriceRecordExists(int id)
        {
            return _context.PriceRecords.Any(e => e.Id == id);
        }
    }
}
