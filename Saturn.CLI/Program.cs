#define DEBUG

using Saturn.BL;
using Saturn.BL.FeatureUtils;
using Saturn.BL.Logging;
using Saturn.BL.Persistence;
using Saturn.Persistance;
using Serilog;
using Serilog.Core;

class Program
{
    public static Logger? _logger;

    private static FeatureHandler? featureHandler;

    static async Task Main(string[] args)
    {
        try
        {
            var loggerConfiguration = new LoggerConfiguration()
             .WriteTo.File(AppInfo.LogFilePath_CLI);

            _logger = loggerConfiguration.CreateLogger();

            var loggerLogicProvider = new LoggerLogicProviderSerilog(_logger);

            featureHandler = await FeatureHandler.Build(loggerLogicProvider.LogInformation, loggerLogicProvider.LogWarning);

            await CommandHandler.Parse(featureHandler, args);

            if (featureHandler.IsModified)
            {
                await Cache.Save(featureHandler.Features);
            }
            if(AppInfo.SaveFeatureOutputToFile)
            {
                await featureHandler.SaveOutputToFile();
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            await Console.Out.WriteLineAsync(ex.Message);
#endif

            _logger?.Fatal(ex.Message);
        }
    }
}