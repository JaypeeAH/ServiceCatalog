using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCatalog.Web.Data;
using ServiceCatalog.Web.Models;

namespace ServiceCatalog.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class LocationsApiController : ControllerBase
    {
        private readonly ServiceCatalogDbContext _context;
        private readonly ILogger<LocationsApiController> _logger;

        public LocationsApiController(ServiceCatalogDbContext context, ILogger<LocationsApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all locations
        /// </summary>
        /// <returns>List of locations</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
        {
            _logger.LogInformation("API: Getting all locations");
            return await _context.Locations.ToListAsync();
        }

        /// <summary>
        /// Get a specific location by ID
        /// </summary>
        /// <param name="id">Location ID</param>
        /// <returns>Location details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Location>> GetLocation(int id)
        {
            _logger.LogInformation("API: Getting location {Id}", id);
            var location = await _context.Locations.FindAsync(id);

            if (location == null)
            {
                _logger.LogWarning("API: Location {Id} not found", id);
                return NotFound(new { message = $"Location with ID {id} not found" });
            }

            return location;
        }

        /// <summary>
        /// Get locations by region
        /// </summary>
        /// <param name="region">Region name (EMEA, APAC, Americas)</param>
        /// <returns>List of locations in the region</returns>
        [HttpGet("by-region/{region}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocationsByRegion(string region)
        {
            _logger.LogInformation("API: Getting locations for region {Region}", region);
            return await _context.Locations
                .Where(l => l.Region == region)
                .ToListAsync();
        }

        /// <summary>
        /// Create a new location
        /// </summary>
        /// <param name="location">Location data</param>
        /// <returns>Created location</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Location>> CreateLocation(Location location)
        {
            _logger.LogInformation("API: Creating new location: {Name}", location.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _context.Locations.Add(location);
                await _context.SaveChangesAsync();

                _logger.LogInformation("API: Location created with ID: {Id}", location.Id);
                return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error creating location");
                return BadRequest(new { message = "Error creating location", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing location
        /// </summary>
        /// <param name="id">Location ID</param>
        /// <param name="location">Updated location data</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLocation(int id, Location location)
        {
            if (id != location.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            _logger.LogInformation("API: Updating location {Id}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _context.Entry(location).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("API: Location {Id} updated successfully", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await LocationExists(id))
                {
                    _logger.LogWarning("API: Location {Id} not found for update", id);
                    return NotFound(new { message = $"Location with ID {id} not found" });
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error updating location {Id}", id);
                return BadRequest(new { message = "Error updating location", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a location
        /// </summary>
        /// <param name="id">Location ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            _logger.LogInformation("API: Deleting location {Id}", id);

            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                _logger.LogWarning("API: Location {Id} not found for deletion", id);
                return NotFound(new { message = $"Location with ID {id} not found" });
            }

            try
            {
                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();

                _logger.LogInformation("API: Location {Id} deleted successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error deleting location {Id}", id);
                return BadRequest(new { message = "Error deleting location", error = ex.Message });
            }
        }

        private async Task<bool> LocationExists(int id)
        {
            return await _context.Locations.AnyAsync(e => e.Id == id);
        }
    }
}
