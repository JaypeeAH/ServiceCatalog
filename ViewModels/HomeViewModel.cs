using ServiceCatalog.Web.Models;

namespace ServiceCatalog.Web.ViewModels
{
    public class HomeViewModel
    {
    public List<Capability> Capabilities { get; set; } = new();
  public List<Service> Services { get; set; } = new();
        public List<Location> Locations { get; set; } = new();
 public string SelectedCategory { get; set; } = "All";
    }
}
