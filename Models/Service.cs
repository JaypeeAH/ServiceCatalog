using System.ComponentModel.DataAnnotations;

namespace ServiceCatalog.Web.Models
{
    public class Service
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Service name is required")]
        public string Name { get; set; } = string.Empty;
        
        // Foreign key to Capability
        [Required(ErrorMessage = "Please select a capability")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a capability")]
        public int CapabilityId { get; set; }
        
        public string Category { get; set; } = string.Empty; // Keeping for backward compatibility temporarily
     
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Display order is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Order must be at least 1")]
        public int Order { get; set; } // For custom ordering within categories
        
        // Navigation properties - nullable to avoid model binding validation errors
        public Capability? Capability { get; set; } = null;
        public ICollection<ServiceProcess> Processes { get; set; } = new List<ServiceProcess>();
    }
}
