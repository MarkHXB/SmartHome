using Saturn.BL.AppConfig;
using Saturn.BL.FeatureUtils;
using Saturn.BL.Logging;
using Saturn.Persistance;

namespace Saturn.BL
{
    public enum RunMode
    {
        DEFAULT,
        MENU,
        CLI
    }
    public class VirtualBox 
    {
        private string[] _args;
        private RunMode _runMode;

        private static VirtualBox VirtualBoxInstance;
   
        private VirtualBox()
        {
            LoggerLogicProvider = new LoggerLogicProviderSerilog();
            FeatureHandler = FeatureHandler.BuildAsync(LoggerLogicProvider).GetAwaiter().GetResult();
        }
        public VirtualBox(string[] args, RunMode runMode = RunMode.DEFAULT) : this()
        {
            _args = args;
            _runMode = runMode;   
        }

        public ILoggerLogicProvider LoggerLogicProvider { get; }
        public FeatureHandler FeatureHandler { get; }

        public static VirtualBox GetInstance()
        {
            if(VirtualBoxInstance is null)
            {
                VirtualBoxInstance = new VirtualBox();
            }

            return VirtualBoxInstance;
        }

        public async Task Run()
        {
            await Task.Run(async () =>
            {
                switch (_runMode)
                {
                    case RunMode.MENU:
                        await CallMenu();
                        break;
                    case RunMode.CLI:
                        await CallCli();
                        break;
                    default:
                        await CallDefault();
                        break;
                }

                if (FeatureHandler.IsModified)
                {
                    await Cache.Save(FeatureHandler.GetFeatures());
                }
                if (AppInfoResolver.ShouldSaveFeatureOutputToFile())
                {
                    await FeatureHandler.SaveOutputToFile();
                }

                await ConfigHandler.Save();
            });
        }

        private async Task CallDefault()
        {

        }

        private async Task CallCli()
        {           
            try
            {
                await CommandHandler.Parse(FeatureHandler, _args);            
            }
            catch (Exception ex)
            {
                LoggerLogicProvider.LogWarning(ex.Message);
            }
        }

        private async Task CallMenu()
        {
            await new Menu().Run();
        }
    }
}
