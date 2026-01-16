using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCatalog.Web.Data;
using ServiceCatalog.Web.Models;

namespace ServiceCatalog.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ServicesApiController : ControllerBase
    {
        private readonly ServiceCatalogDbContext _context;
        private readonly ILogger<ServicesApiController> _logger;

        public ServicesApiController(ServiceCatalogDbContext context, ILogger<ServicesApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all services
        /// </summary>
        /// <returns>List of services</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            _logger.LogInformation("API: Getting all services");
            return await _context.Services
                .Include(s => s.Capability)
                .Include(s => s.Processes)
                .OrderBy(s => s.Order)
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific service by ID
        /// </summary>
        /// <param name="id">Service ID</param>
        /// <returns>Service details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            _logger.LogInformation("API: Getting service {Id}", id);
            var service = await _context.Services
                .Include(s => s.Capability)
                .Include(s => s.Processes)
                    .ThenInclude(p => p.Applicabilities)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                _logger.LogWarning("API: Service {Id} not found", id);
                return NotFound(new { message = $"Service with ID {id} not found" });
            }

            return service;
        }

        /// <summary>
        /// Get services by capability
        /// </summary>
        /// <param name="capabilityId">Capability ID</param>
        /// <returns>List of services for the capability</returns>
        [HttpGet("by-capability/{capabilityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Service>>> GetServicesByCapability(int capabilityId)
        {
            _logger.LogInformation("API: Getting services for capability {CapabilityId}", capabilityId);
            return await _context.Services
                .Where(s => s.CapabilityId == capabilityId)
                .Include(s => s.Capability)
                .Include(s => s.Processes)
                .OrderBy(s => s.Order)
                .ToListAsync();
        }

        /// <summary>
        /// Create a new service
        /// </summary>
        /// <param name="service">Service data</param>
        /// <returns>Created service</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Service>> CreateService(Service service)
        {
            _logger.LogInformation("API: Creating new service: {Name}", service.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Verify CapabilityId exists
                var capabilityExists = await _context.Capabilities.AnyAsync(c => c.Id == service.CapabilityId);
                if (!capabilityExists)
                {
                    return BadRequest(new { message = $"Capability with ID {service.CapabilityId} does not exist" });
                }

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                _logger.LogInformation("API: Service created with ID: {Id}", service.Id);
                
                // Reload with navigation properties
                var createdService = await _context.Services
                    .Include(s => s.Capability)
                    .FirstOrDefaultAsync(s => s.Id == service.Id);

                return CreatedAtAction(nameof(GetService), new { id = service.Id }, createdService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error creating service");
                return BadRequest(new { message = "Error creating service", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing service
        /// </summary>
        /// <param name="id">Service ID</param>
        /// <param name="service">Updated service data</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateService(int id, Service service)
        {
            if (id != service.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            _logger.LogInformation("API: Updating service {Id}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Verify CapabilityId exists
                var capabilityExists = await _context.Capabilities.AnyAsync(c => c.Id == service.CapabilityId);
                if (!capabilityExists)
                {
                    return BadRequest(new { message = $"Capability with ID {service.CapabilityId} does not exist" });
                }

                _context.Entry(service).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("API: Service {Id} updated successfully", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ServiceExists(id))
                {
                    _logger.LogWarning("API: Service {Id} not found for update", id);
                    return NotFound(new { message = $"Service with ID {id} not found" });
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error updating service {Id}", id);
                return BadRequest(new { message = "Error updating service", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a service
        /// </summary>
        /// <param name="id">Service ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteService(int id)
        {
            _logger.LogInformation("API: Deleting service {Id}", id);

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                _logger.LogWarning("API: Service {Id} not found for deletion", id);
                return NotFound(new { message = $"Service with ID {id} not found" });
            }

            try
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                _logger.LogInformation("API: Service {Id} deleted successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error deleting service {Id}", id);
                return BadRequest(new { message = "Error deleting service", error = ex.Message });
            }
        }

        private async Task<bool> ServiceExists(int id)
        {
            return await _context.Services.AnyAsync(e => e.Id == id);
        }
    }
}
