using System.ComponentModel.DataAnnotations;

namespace ServiceCatalog.Web.Models
{
    public class Capability
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Capability name is required")]
        public string Name { get; set; } = string.Empty; // Customer Service, Human Resource, Clinical Services, etc.
        
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Display order is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Order must be at least 1")]
        public int Order { get; set; }
        
        [Required(ErrorMessage = "Color is required")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Please enter a valid hex color (e.g., #EC5A62)")]
        public string ColorHex { get; set; } = "#EC5A62"; // Default Cardinal Red
        
        // Navigation property
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
