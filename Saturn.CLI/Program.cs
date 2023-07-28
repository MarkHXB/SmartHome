#define DEBUG

using Saturn.BL;
using Saturn.BL.FeatureUtils;
using Saturn.BL.Logging;
using Saturn.CLI;
using Saturn.Persistance;
using Serilog;
using Serilog.Core;

class Program
{
    //public static Logger? _logger;

    //private static FeatureHandler? featureHandler;
    static Menu menu = new Menu();
    static async Task Main(string[] args)
    {
        await menu.Run();

        //        try
        //        {

        //ILoggerLogicProvider loggerLogicProvider = new LoggerLogicProviderSerilog();

        //            featureHandler = await FeatureHandler.BuildAsync(loggerLogicProvider);

        //            await CommandHandler.Parse(featureHandler, args);

        //            if (featureHandler.IsModified)
        //            {
        //                await Cache.Save(featureHandler.GetFeatures());
        //            }
        //            if (AppInfoResolver.ShouldSaveFeatureOutputToFile())
        //            {
        //                await featureHandler.SaveOutputToFile();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //#if DEBUG
        //            await Console.Out.WriteLineAsync(ex.Message);
        //#endif

        //            _logger?.Fatal(ex.Message);
        //        }
    }
}