using System.ComponentModel.DataAnnotations;

namespace ServiceCatalog.Web.Models
{
    public class Location
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Location name is required")]
        public string Name { get; set; } = string.Empty; // e.g., Cardinal Health GBFS US, Cardinal Health International Philippines
        
        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Region is required")]
        public string Region { get; set; } = string.Empty; // e.g., EMEA, APAC, Americas
        
        public string FlagUrl { get; set; } = string.Empty; // Path to flag image
        
        [Required(ErrorMessage = "Employee count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Employee count must be 0 or greater")]
        public int EmployeeCount { get; set; }
        
        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }
        
        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }
    }
}
