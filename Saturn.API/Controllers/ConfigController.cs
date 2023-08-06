using Microsoft.AspNetCore.Mvc;
using Saturn.BL.AppConfig;

namespace Saturn.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ConfigController : Controller
    {
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(ILogger<ConfigController> logger)
        {
            _logger = logger;
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
        public IActionResult GetConfig(string name)
        {
            var configs = ConfigHandler.GetAllConfig().Select(config => config.Value.FirstOrDefault(v => v.Item1.ToLower().Equals(name.ToLower().Trim())));

            _logger.LogInformation(" @API GetConfig was made successfully. Call from: Config/GetConfig?name...");

            return Ok(configs);
        }
    }
}
