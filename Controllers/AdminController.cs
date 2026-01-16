using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCatalog.Web.Data;
using ServiceCatalog.Web.Models;

namespace ServiceCatalog.Web.Controllers
{
    // NOTE: In production, add [Authorize] attribute for secure access
    // [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ServiceCatalogDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ServiceCatalogDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
   _logger = logger;
    }

      // Dashboard
        public async Task<IActionResult> Index()
     {
        var stats = new
          {
                TotalServices = await _context.Services.CountAsync(),
       TotalProcesses = await _context.ServiceProcesses.CountAsync(),
                TotalLocations = await _context.Locations.CountAsync(),
                TotalCategories = await _context.Capabilities.CountAsync() // Changed from TotalCapabilities to TotalCategories to match view
     };

          ViewBag.Stats = stats;
       return View();
        }

        #region Service Management

// GET: Admin/Services
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services
      .Include(s => s.Capability)
                .Include(s => s.Processes)
       .ThenInclude(p => p.Applicabilities) // CHANGED: Include applicabilities through processes
        .OrderBy(s => s.Order)
  .ToListAsync();
            return View(services);
        }

        // GET: Admin/CreateService
        public async Task<IActionResult> CreateService()
   {
       ViewBag.Capabilities = await _context.Capabilities.OrderBy(c => c.Order).ToListAsync();
            return View();
        }

  // POST: Admin/CreateService
        [HttpPost]
   [ValidateAntiForgeryToken]
     public async Task<IActionResult> CreateService(Service service)
 {
            // Log incoming request
            _logger.LogInformation("CreateService POST called - ModelState.IsValid: {IsValid}", ModelState.IsValid);
            _logger.LogInformation("Service Details - Name: {Name}, CapabilityId: {CapabilityId}, Order: {Order}", 
                service.Name, service.CapabilityId, service.Order);

       if (ModelState.IsValid)
          {
                try
                {
                    // Verify CapabilityId exists before trying to save
                    var capabilityExists = await _context.Capabilities.AnyAsync(c => c.Id == service.CapabilityId);
                    if (!capabilityExists)
                    {
                        _logger.LogError("Invalid CapabilityId: {CapabilityId} - Capability does not exist", service.CapabilityId);
                        ModelState.AddModelError("CapabilityId", $"The selected capability (ID: {service.CapabilityId}) does not exist.");
                        ViewBag.Capabilities = await _context.Capabilities.OrderBy(c => c.Order).ToListAsync();
                        return View(service);
                    }

                    _context.Services.Add(service);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Service saved with ID: {ServiceId}", service.Id);

                    // Note: Applicabilities are now created at the ServiceProcess level, not Service level
                    // When creating processes for this service, applicabilities should be created there
                    
                    TempData["Success"] = "Service created successfully! Remember to add processes and their applicabilities.";
                    return RedirectToAction(nameof(Services));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database error creating service - Foreign key constraint violation likely");
                    
                    // Check for foreign key constraint error
                    if (ex.InnerException?.Message.Contains("FOREIGN KEY constraint") == true)
                    {
                        TempData["Error"] = $"Cannot create service: The selected capability does not exist in the database. Please select a valid capability.";
                    }
                    else
                    {
                        TempData["Error"] = $"Database error creating service: {ex.InnerException?.Message ?? ex.Message}";
                    }
                    
                    ViewBag.Capabilities = await _context.Capabilities.OrderBy(c => c.Order).ToListAsync();
                    return View(service);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating service");
                    TempData["Error"] = $"Error creating service: {ex.Message}";
                    ViewBag.Capabilities = await _context.Capabilities.OrderBy(c => c.Order).ToListAsync();
                    return View(service);
                }
    }
            
            // Log validation errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("ModelState Error: {ErrorMessage}", error.ErrorMessage);
            }
            
   ViewBag.Capabilities = await _context.Capabilities.OrderBy(c => c.Order).ToListAsync();
  return View(service);
        }

      // GET: Admin/EditService/5
      public async Task<IActionResult> EditService(int? id)
        {
         if (id == null) return NotFound();

  var service = await _context.Services
     .Include(s => s.Capability)
                .FirstOrDefaultAsync(s => s.Id == id);

       if (service == null) return NotFound();

            ViewBag.Capabilities = await _context.Capabilities.OrderBy(c => c.Order).ToListAsync();
 return View(service);
        }

     // POST: Admin/EditService/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, Service service)
        {
            if (id != service.Id) return NotFound();

     if (ModelState.IsValid)
        {
    try
                {
            _context.Update(service);
   await _context.SaveChangesAsync();
    TempData["Success"] = "Service updated successfully!";
   return RedirectToAction(nameof(Services));
  }
        catch (DbUpdateConcurrencyException)
    {
            if (!ServiceExists(service.Id))
            return NotFound();
        throw;
  }
            }
   ViewBag.Capabilities = await _context.Capabilities.OrderBy(c => c.Order).ToListAsync();
 return View(service);
        }

        // POST: Admin/DeleteService/5
  [HttpPost]
        [ValidateAntiForgeryToken]
   public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
       if (service != null)
   {
          _context.Services.Remove(service);
      await _context.SaveChangesAsync();
                TempData["Success"] = "Service deleted successfully!";
            }
          return RedirectToAction(nameof(Services));
     }

      #endregion

      #region Process Management

        // GET: Admin/Processes
   public async Task<IActionResult> Processes()
        {
            var processes = await _context.ServiceProcesses
        .Include(p => p.Service)
       .Include(p => p.Applicabilities) // Include applicabilities for each process
           .OrderBy(p => p.ServiceId)
                .ThenBy(p => p.Order)
        .ToListAsync();
          return View(processes);
        }

        // GET: Admin/CreateProcess
        public async Task<IActionResult> CreateProcess()
  {
            ViewBag.Services = await _context.Services.ToListAsync();
      return View();
        }

        // POST: Admin/CreateProcess
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProcess(ServiceProcess process)
        {
            // Log incoming request
            _logger.LogInformation("CreateProcess POST called - ModelState.IsValid: {IsValid}", ModelState.IsValid);
            _logger.LogInformation("Process Details - ServiceId: {ServiceId}, Name: {Name}, Order: {Order}", 
                process.ServiceId, process.Name, process.Order);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.ServiceProcesses.Add(process);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Process saved with ID: {ProcessId}", process.Id);

                    // Create default applicability records for this process
                    var divisions = new[] { "Medical", "Pharma", "AHS", "OFL" };
                    foreach (var division in divisions)
                    {
                        _context.ServiceApplicabilities.Add(new ServiceApplicability
                        {
                            ServiceProcessId = process.Id,
                            Division = division,
                            IsApplicable = true
                        });
                    }
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Applicabilities created for process ID: {ProcessId}", process.Id);

                    TempData["Success"] = "Process created successfully with default applicabilities!";
                    return RedirectToAction(nameof(Processes));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating process");
                    TempData["Error"] = $"Error creating process: {ex.Message}";
                    ViewBag.Services = await _context.Services.ToListAsync();
                    return View(process);
                }
            }
            
            // Log validation errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("ModelState Error: {ErrorMessage}", error.ErrorMessage);
            }
            
            ViewBag.Services = await _context.Services.ToListAsync();
            return View(process);
        }

    // POST: Admin/DeleteProcess/5
     [HttpPost]
     [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProcess(int id)
        {
       var process = await _context.ServiceProcesses.FindAsync(id);
   if (process != null)
       {
     _context.ServiceProcesses.Remove(process);
        await _context.SaveChangesAsync();
         TempData["Success"] = "Process deleted successfully!";
            }
            return RedirectToAction(nameof(Processes));
 }

      #endregion

        #region Location Management

        // GET: Admin/Locations
        public async Task<IActionResult> Locations()
      {
     var locations = await _context.Locations.ToListAsync();
return View(locations);
        }

    // GET: Admin/CreateLocation
public IActionResult CreateLocation()
   {
   return View();
        }

     // POST: Admin/CreateLocation
   [HttpPost]
        [ValidateAntiForgeryToken]
      public async Task<IActionResult> CreateLocation(Location location)
        {
            // Log incoming request
            _logger.LogInformation("CreateLocation POST called - ModelState.IsValid: {IsValid}", ModelState.IsValid);
            _logger.LogInformation("Location Details - Name: {Name}, Country: {Country}, Region: {Region}", 
                location.Name, location.Country, location.Region);

          if (ModelState.IsValid)
     {
                try
                {
                    _context.Locations.Add(location);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Location saved with ID: {LocationId}", location.Id);
                    
                    TempData["Success"] = "Location created successfully!";
                    return RedirectToAction(nameof(Locations));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating location");
                    TempData["Error"] = $"Error creating location: {ex.Message}";
                    return View(location);
                }
 }
            
            // Log validation errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("ModelState Error: {ErrorMessage}", error.ErrorMessage);
            }
            
       return View(location);
        }

        // GET: Admin/EditLocation/5
        public async Task<IActionResult> EditLocation(int? id)
        {
            if (id == null) return NotFound();

            var location = await _context.Locations.FindAsync(id);
      if (location == null) return NotFound();

      return View(location);
        }

        // POST: Admin/EditLocation/5
     [HttpPost]
        [ValidateAntiForgeryToken]
     public async Task<IActionResult> EditLocation(int id, Location location)
        {
            if (id != location.Id) return NotFound();

            if (ModelState.IsValid)
       {
  try
            {
           _context.Update(location);
  await _context.SaveChangesAsync();
 TempData["Success"] = "Location updated successfully!";
                 return RedirectToAction(nameof(Locations));
     }
      catch (DbUpdateConcurrencyException)
  {
        if (!await _context.Locations.AnyAsync(l => l.Id == id))
       return NotFound();
          throw;
      }
   }
   return View(location);
        }

        // POST: Admin/DeleteLocation/5
 [HttpPost]
        [ValidateAntiForgeryToken]
 public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location != null)
          {
       _context.Locations.Remove(location);
 await _context.SaveChangesAsync();
    TempData["Success"] = "Location deleted successfully!";
    }
       return RedirectToAction(nameof(Locations));
        }

  #endregion

  #region Capability Management

        // GET: Admin/Capabilities
  public async Task<IActionResult> Capabilities()
        {
   var capabilities = await _context.Capabilities
              .Include(c => c.Services)
    .OrderBy(c => c.Order)
         .ToListAsync();
            return View(capabilities);
        }

        // GET: Admin/CreateCapability
        public IActionResult CreateCapability()
        {
     return View();
        }

    // POST: Admin/CreateCapability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCapability(Capability capability)
        {
   if (ModelState.IsValid)
     {
        _context.Capabilities.Add(capability);
                await _context.SaveChangesAsync();
   TempData["Success"] = "Capability created successfully!";
    return RedirectToAction(nameof(Capabilities));
          }
   return View(capability);
        }

  // GET: Admin/EditCapability/5
    public async Task<IActionResult> EditCapability(int? id)
      {
            if (id == null) return NotFound();

       var capability = await _context.Capabilities.FindAsync(id);
     if (capability == null) return NotFound();

            return View(capability);
        }

        // POST: Admin/EditCapability/5
        [HttpPost]
      [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCapability(int id, Capability capability)
 {
            if (id != capability.Id) return NotFound();

            if (ModelState.IsValid)
        {
             try
          {
              _context.Update(capability);
         await _context.SaveChangesAsync();
 TempData["Success"] = "Capability updated successfully!";
       return RedirectToAction(nameof(Capabilities));
           }
                catch (DbUpdateConcurrencyException)
       {
         if (!await _context.Capabilities.AnyAsync(c => c.Id == id))
       return NotFound();
         throw;
          }
 }
            return View(capability);
        }

        // POST: Admin/DeleteCapability/5
        [HttpPost]
        [ValidateAntiForgeryToken]
      public async Task<IActionResult> DeleteCapability(int id)
        {
  var capability = await _context.Capabilities.FindAsync(id);
         if (capability != null)
            {
     // Check if there are services using this capability
      var hasServices = await _context.Services.AnyAsync(s => s.CapabilityId == id);
              if (hasServices)
    {
       TempData["Error"] = "Cannot delete capability that has associated services. Delete or reassign the services first.";
        return RedirectToAction(nameof(Capabilities));
  }

_context.Capabilities.Remove(capability);
           await _context.SaveChangesAsync();
      TempData["Success"] = "Capability deleted successfully!";
            }
   return RedirectToAction(nameof(Capabilities));
        }

      #endregion

        #region ServiceProcess Edit

        // GET: Admin/EditProcess/5
        public async Task<IActionResult> EditProcess(int? id)
        {
            if (id == null) return NotFound();

            var process = await _context.ServiceProcesses
    .Include(p => p.Service)
       .FirstOrDefaultAsync(p => p.Id == id);

 if (process == null) return NotFound();

 ViewBag.Services = await _context.Services.ToListAsync();
            return View(process);
        }

        // POST: Admin/EditProcess/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProcess(int id, ServiceProcess process)
     {
 if (id != process.Id) return NotFound();

         if (ModelState.IsValid)
  {
                try
             {
       _context.Update(process);
 await _context.SaveChangesAsync();
       TempData["Success"] = "Process updated successfully!";
    return RedirectToAction(nameof(Processes));
   }
      catch (DbUpdateConcurrencyException)
           {
        if (!await _context.ServiceProcesses.AnyAsync(p => p.Id == id))
                return NotFound();
                    throw;
}
            }
            ViewBag.Services = await _context.Services.ToListAsync();
            return View(process);
   }

        #endregion

        #region ServiceApplicability Management

        // GET: Admin/Applicabilities
        public async Task<IActionResult> Applicabilities()
        {
       var applicabilities = await _context.ServiceApplicabilities
 .Include(a => a.ServiceProcess)
           .ThenInclude(p => p.Service)
              .OrderBy(a => a.ServiceProcess.ServiceId)
       .ThenBy(a => a.ServiceProcessId)
         .ThenBy(a => a.Division)
         .ToListAsync();
         return View(applicabilities);
        }

        // GET: Admin/EditApplicability/5
        public async Task<IActionResult> EditApplicability(int? id)
   {
         if (id == null) return NotFound();

   var applicability = await _context.ServiceApplicabilities
                .Include(a => a.ServiceProcess)
         .ThenInclude(p => p.Service)
       .FirstOrDefaultAsync(a => a.Id == id);

            if (applicability == null) return NotFound();

    return View(applicability);
        }

        // POST: Admin/EditApplicability/5
        [HttpPost]
      [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditApplicability(int id, ServiceApplicability applicability)
        {
            if (id != applicability.Id) return NotFound();

            if (ModelState.IsValid)
            {
            try
{
     _context.Update(applicability);
    await _context.SaveChangesAsync();
          TempData["Success"] = "Applicability updated successfully!";
     return RedirectToAction(nameof(Applicabilities));
      }
          catch (DbUpdateConcurrencyException)
   {
      if (!await _context.ServiceApplicabilities.AnyAsync(a => a.Id == id))
 return NotFound();
   throw;
 }
            }
            return View(applicability);
  }

      #endregion

       #region Helper Methods

   private bool ServiceExists(int id)
        {
return _context.Services.Any(e => e.Id == id);
        }

        #endregion
    }
}
