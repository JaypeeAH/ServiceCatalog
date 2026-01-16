using System.ComponentModel.DataAnnotations;

namespace ServiceCatalog.Web.Models
{
    public class ServiceProcess
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Please select a service")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a service")]
        public int ServiceId { get; set; }
        
        [Required(ErrorMessage = "Process name is required")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;
        
        public string Category { get; set; } = string.Empty; // e.g., Customer Inquiry Management, Order Management Support
        
        [Required(ErrorMessage = "Display order is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Order must be at least 1")]
        public int Order { get; set; }
        
        // Navigation properties - nullable to avoid model binding validation errors
        public Service? Service { get; set; } = null;
        public ICollection<ServiceApplicability> Applicabilities { get; set; } = new List<ServiceApplicability>();
    }
}
