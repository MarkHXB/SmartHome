using Saturn.BL;
using Saturn.BL.FeatureUtils;
using Saturn.BL.Logging;

namespace Saturn.Tests
{
    public class FeatureStopTests
    {
        [Fact(Skip = "Not CI/CD friendly")]
        public async void WhetherTaskIsStoppedWhenItRequestedInDeterminedEnv()
        {
            string featureName = "TimeEventSourcer";
            ILoggerLogicProvider loggerLogicProvider = new LoggerLogicProviderSerilog();
            var featureHandler = await FeatureHandler.BuildAsync(loggerLogicProvider);

            Task.Run(() => CommandHandler.Parse(featureHandler, "run", featureName));

            Thread.Sleep(2000);

            Task.Run(() => CommandHandler.Parse(featureHandler, "stop", featureName));

            Thread.Sleep(4000);

            var result = featureHandler.GetFeatures().FirstOrDefault(f => f.FeatureName == featureName)?.IsRunning ?? false;

            Xunit.Assert.False(result);
        }
    }
}
