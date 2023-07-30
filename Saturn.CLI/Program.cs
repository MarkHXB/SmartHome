#define DEBUG

using Saturn.BL;
using Saturn.BL.FeatureUtils;
using Saturn.BL.Logging;
using Saturn.CLI;
using Saturn.Persistance;

class Program
{
    static async Task Main(string[] args)
    {
        string runMode = args.FirstOrDefault() ?? string.Empty;
        var tempList = args.ToList();
        tempList.Remove(runMode);
        args = tempList.ToArray();

        switch (runMode.ToLower().Trim())
        {
            case "menu":
                await CallMenu();
                break;
            case "cli":
                await CallCli(args);
                break;
            default:
                break;
        }
    }

    private static async Task CallCli(string[] args)
    {
        FeatureHandler? featureHandler;

        try
        {

            ILoggerLogicProvider loggerLogicProvider = new LoggerLogicProviderSerilog();

            featureHandler = await FeatureHandler.BuildAsync(loggerLogicProvider);

            await CommandHandler.Parse(featureHandler, args);

            if (featureHandler.IsModified)
            {
                await Cache.Save(featureHandler.GetFeatures());
            }
            if (AppInfoResolver.ShouldSaveFeatureOutputToFile())
            {
                await featureHandler.SaveOutputToFile();
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            await Console.Out.WriteLineAsync(ex.Message);
#endif
        }
    }
    private static async Task CallMenu()
    {
        Menu menu = new Menu();
        await menu.Run();
    }
}