
using Microsoft.AspNetCore.Mvc;
using ConfigurationReader;

namespace ConfigurationAdminUI.Controllers
{
    public class ConfigTestController : Controller
    {
        private readonly ConfigurationReader.ConfigurationReader reader;
        private readonly ConfigurationReader.ConfigurationReader _reader;

        public ConfigTestController(ConfigurationReader.ConfigurationReader reader)
        {
            _reader = reader;
        }

        public IActionResult Index()
        {
            try
            {
                string siteName = reader.GetValue<string>("SiteName");
                //bool isBasketEnabled = reader.GetValue<bool>("IsBasketEnabled");
                int maxItemCount = reader.GetValue<int>("MaxItemCount");


                return Content($"SiteName: {siteName} | MaxItemCount: {maxItemCount}");
            }
            catch (Exception ex)
            {
                return Content($"Hata: {ex.Message}");
            }
        }
    }
}
