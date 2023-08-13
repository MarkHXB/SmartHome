using Microsoft.AspNetCore.Mvc;
using Saturn.BL;
using Saturn.BL.AppConfig;
using Saturn.BL.FeatureUtils;

namespace Saturn.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ConfigController : Controller
    {
        private readonly FeatureHandler _featureHandler;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(ILogger<ConfigController> logger)
        {
            _logger = logger;
            _featureHandler = VirtualBox.GetInstance().FeatureHandler;
            ConfigHandler.Build(_featureHandler.LogInformation);
        }

        [HttpGet]
        public IActionResult GetConfigs()
        {
            var configs = ConfigHandler.GetAllConfig();

            if (configs is null)
            {
                _logger.LogWarning(" @API Not found any features on HttpGet features. Call from: Config/GetConfigs");

                return NotFound();
            }

            _logger.LogInformation(" @API GetConfig was made successfully. Call from: Config/GetConfigs");

            return Ok(configs);
        }

        [HttpGet]
        public IActionResult GetConfigProperty(string propertyName)
        {
            var config = ConfigHandler.GetAllConfig().Values.FirstOrDefault(kv => kv.Any(k => k.Key.Contains(propertyName)))?.First();

            if (config is null)
            {
                _logger.LogWarning(" @API Not found any features on HttpGet features. Call from: Config/GetConfigProperty");

                return NotFound();
            }

            _logger.LogInformation(" @API GetConfig was made successfully. Call from: Config/GetConfig?name...");

            return Ok(config);
        }

        [HttpPost]
        public IActionResult SetConfigProperty(string propertyName, string type, string value)
        {
            try
            {
                ConfigHandler.SetConfig(propertyName, type, value);
            }
            catch (Exception ex)
            {
                return NotFound(ex);
            }

            _logger.LogInformation(" @API SetConfig was made successfully. Call from: Config/SetConfigProperty?name...&value...");

            return Ok($"You have successfully set {propertyName} to {value}");
        }
    }
}
