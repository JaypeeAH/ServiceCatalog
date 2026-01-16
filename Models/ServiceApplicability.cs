using System.ComponentModel.DataAnnotations;

namespace ServiceCatalog.Web.Models
{
    public class ServiceApplicability
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Process is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a process")]
        public int ServiceProcessId { get; set; } // Changed from ServiceId to ServiceProcessId
        
        [Required(ErrorMessage = "Division is required")]
        public string Division { get; set; } = string.Empty; // Medical, Pharma, AHS, OFL
        
        public bool IsApplicable { get; set; }
        
        // Navigation property - nullable to avoid model binding validation errors
        public ServiceProcess? ServiceProcess { get; set; } = null; // Changed from Service to ServiceProcess
    }
}
