using Microsoft.AspNetCore.Mvc;
using Saturn.BL;
using Saturn.BL.FeatureUtils;
using Saturn.Shared;

namespace Saturn.API.Controllers
{
    enum LogLevel
    {
        LogInformation,
        LogWarning
    }

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FeaturesController : Controller
    {
        private readonly FeatureHandler _featureHandler;
        private readonly ILogger<FeaturesController> _logger;

        public FeaturesController(ILogger<FeaturesController> logger)
        {
            _logger = logger;
            _featureHandler = VirtualBox.GetInstance(RunMode.WEBAPI).FeatureHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var features = await _featureHandler.GetFeaturesAsync();
                return Ok(features);
            }
            catch (Exception ex)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(string? featureName)
        {
            try
            {
                var features = await _featureHandler.GetFeaturesAsync();
                var feature = features.FirstOrDefault(f => f.Name == featureName);
                return Ok(feature);
            }
            catch (Exception ex)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string? featureName)
        {
            try
            {
                await Task.Run(() =>
                {
                    _featureHandler.EnableFeature(featureName);
                });

                return Ok($"Feature {featureName} enabled.");
            }
            catch (Exception ex)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Disable(string? featureName)
        {
            try
            {
                await Task.Run(() =>
                {
                    _featureHandler.DisableFeature(featureName);
                });

                return Ok($"Feature {featureName} disabled.");
            }
            catch (Exception ex)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Run(string? featureName, List<KeyValuePair<string,string>>? args)
        {
            try
            {
                string output = await _featureHandler.TryToRunReturnOutput(featureName, GeneretaArgsFromList(args));
                return Ok(output);
            }
            catch (Exception ex)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult RunAll()
        {
            try
            {
                Task.Run(() => _featureHandler.TryToRunAll());
                return Ok("Run all features operation started");
            }
            catch (Exception ex)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Stop(string? featureName)
        {
            try
            {
                await Task.Run(() => _featureHandler.Stop(featureName));

                return Ok($"Stopped {featureName} successfully.");
            }
            catch (Exception ex)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> StopAll()
        {
            try
            {
                await Task.Run(() => _featureHandler.StopAll());

                return Ok($"Stopped all features successfully.");
            }
            catch (Exception ex)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API {ex.Message}");
            }
        }

        private BadRequestObjectResult ReportExceptionToLog(LogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case LogLevel.LogInformation:
                    _logger.LogInformation(message); break;
                case LogLevel.LogWarning:
                    _logger.LogWarning(message); break;
                default:
                    break;
            }

            return BadRequest(message);
        }

        private string[] GeneretaArgsFromList(List<KeyValuePair<string, string>>? args)
        {
            string[] c = Array.Empty<string>();

            args?.ForEach(e =>
            {
                c.Append(e.Key);
                c.Append(e.Value);
            });

            return c;
        }
    }
}
