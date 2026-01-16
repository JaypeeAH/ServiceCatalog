using ServiceCatalog.Web.Models;

namespace ServiceCatalog.Web.ViewModels
{
    public class ServiceDetailViewModel
    {
 public Service Service { get; set; } = null!;
        public List<ServiceProcess> Processes { get; set; } = new();
  public Dictionary<string, bool> Applicabilities { get; set; } = new();
    public List<string> Divisions { get; set; } = new() { "Medical", "Pharma", "AHS", "OEL" };
    }
}
