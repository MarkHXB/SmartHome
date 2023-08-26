using Saturn.BL.FeatureUtils;
using Saturn.BL.Persistence;
using Saturn.Shared;

namespace Saturn.BL
{
    public class VirtualBox
    {
        #region Fields

        private string[] _args;
        private RunMode _runMode;

        private static VirtualBox VirtualBoxInstance;

        #endregion

        #region Constructors

        private VirtualBox(RunMode runMode)
        {
            LoggerLogicProvider = new LoggerLogicProviderSerilog(runMode);
            FeatureHandler = FeatureHandler.BuildAsync(LoggerLogicProvider).GetAwaiter().GetResult();
            _args = Array.Empty<string>();
            _runMode = runMode;
        }
        public VirtualBox(string[] args, RunMode runMode = RunMode.CLI) : this(runMode)
        {
            _args = args;
            _runMode = runMode;
            VirtualBoxInstance = this;
        }

        #endregion

        #region Properties

        public ILoggerLogicProvider LoggerLogicProvider { get; }
        public FeatureHandler FeatureHandler { get; }

        #endregion

        #region Public methods

        public static VirtualBox GetInstance(RunMode runMode)
        {
            if (VirtualBoxInstance is null)
            {
                VirtualBoxInstance = new VirtualBox(runMode);
            }

            return VirtualBoxInstance;
        }

        public async Task Run()
        {
            switch (_runMode)
            {
                case RunMode.MENU:
                    await CallMenu();
                    break;
                case RunMode.CLI:
                    await CallCli(); // fix
                    break;
                case RunMode.MAUI:
                    await CallCli(); // fix
                    break;
                case RunMode.DAEMON:
                    await CallDaemon(); // fix
                    break;
                case RunMode.WEBAPI:
                    await CallCli(); // fix
                    break;
            }

            if (FeatureHandler.IsModified)
            {
                await FeatureHandler.SaveModifiedDescriptionFiles();
            }
            if (AppInfoResolver.ShouldSaveFeatureOutputToFile())
            {
                await FeatureHandler.SaveOutputToFile();
            }

            await ConfigHandler.Save();
        }

      

        #endregion

        #region Private methods

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

        private async Task CallDaemon()
        {
            try
            {
                await CommandHandler.Parse(FeatureHandler, new string[] {"runallscheduled"});

                FeatureScheduler.RefreshScheduling(await FeatureHandler.GetFeaturesAsync());
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
