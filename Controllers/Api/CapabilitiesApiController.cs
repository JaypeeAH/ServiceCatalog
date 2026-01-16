using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCatalog.Web.Data;
using ServiceCatalog.Web.Models;

namespace ServiceCatalog.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class CapabilitiesApiController : ControllerBase
    {
        private readonly ServiceCatalogDbContext _context;
        private readonly ILogger<CapabilitiesApiController> _logger;

        public CapabilitiesApiController(ServiceCatalogDbContext context, ILogger<CapabilitiesApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all capabilities
        /// </summary>
        /// <returns>List of capabilities</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Capability>>> GetCapabilities()
        {
            _logger.LogInformation("API: Getting all capabilities");
            return await _context.Capabilities
                .Include(c => c.Services)
                .OrderBy(c => c.Order)
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific capability by ID
        /// </summary>
        /// <param name="id">Capability ID</param>
        /// <returns>Capability details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Capability>> GetCapability(int id)
        {
            _logger.LogInformation("API: Getting capability {Id}", id);
            var capability = await _context.Capabilities
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (capability == null)
            {
                _logger.LogWarning("API: Capability {Id} not found", id);
                return NotFound(new { message = $"Capability with ID {id} not found" });
            }

            return capability;
        }

        /// <summary>
        /// Create a new capability
        /// </summary>
        /// <param name="capability">Capability data</param>
        /// <returns>Created capability</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Capability>> CreateCapability(Capability capability)
        {
            _logger.LogInformation("API: Creating new capability: {Name}", capability.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _context.Capabilities.Add(capability);
                await _context.SaveChangesAsync();

                _logger.LogInformation("API: Capability created with ID: {Id}", capability.Id);
                return CreatedAtAction(nameof(GetCapability), new { id = capability.Id }, capability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error creating capability");
                return BadRequest(new { message = "Error creating capability", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing capability
        /// </summary>
        /// <param name="id">Capability ID</param>
        /// <param name="capability">Updated capability data</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCapability(int id, Capability capability)
        {
            if (id != capability.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            _logger.LogInformation("API: Updating capability {Id}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _context.Entry(capability).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("API: Capability {Id} updated successfully", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CapabilityExists(id))
                {
                    _logger.LogWarning("API: Capability {Id} not found for update", id);
                    return NotFound(new { message = $"Capability with ID {id} not found" });
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error updating capability {Id}", id);
                return BadRequest(new { message = "Error updating capability", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a capability
        /// </summary>
        /// <param name="id">Capability ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCapability(int id)
        {
            _logger.LogInformation("API: Deleting capability {Id}", id);

            var capability = await _context.Capabilities.FindAsync(id);
            if (capability == null)
            {
                _logger.LogWarning("API: Capability {Id} not found for deletion", id);
                return NotFound(new { message = $"Capability with ID {id} not found" });
            }

            // Check if there are services using this capability
            var hasServices = await _context.Services.AnyAsync(s => s.CapabilityId == id);
            if (hasServices)
            {
                _logger.LogWarning("API: Cannot delete capability {Id} - has associated services", id);
                return BadRequest(new { message = "Cannot delete capability that has associated services" });
            }

            try
            {
                _context.Capabilities.Remove(capability);
                await _context.SaveChangesAsync();

                _logger.LogInformation("API: Capability {Id} deleted successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error deleting capability {Id}", id);
                return BadRequest(new { message = "Error deleting capability", error = ex.Message });
            }
        }

        private async Task<bool> CapabilityExists(int id)
        {
            return await _context.Capabilities.AnyAsync(e => e.Id == id);
        }
    }
}
