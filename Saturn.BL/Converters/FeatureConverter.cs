using Newtonsoft.Json;
using Saturn.Shared;

namespace Saturn.BL.Converters
{
    public static class FeatureConverter
    {
        public static Feature? Convert(string? pathToDescriptionFile)
        {
            Feature? feature = null;

            if (!File.Exists(pathToDescriptionFile))
            {
                return feature;
            }

            string raw = File.ReadAllText(pathToDescriptionFile ?? string.Empty);

            if (AppInfo.IsWindows)
            {
                var converted = JsonConvert.DeserializeObject<FeatureExecutable>(raw);

                if (converted is null) return feature;

                feature = new FeatureExecutable(converted.Name, converted.RunFilePath, converted.Commands, AppInfoResolver.ShouldEnableNewlyAddedFeature(), converted.Cli, converted.WebApi);
            }
            else
            {
                var converted = JsonConvert.DeserializeObject<FeatureDll>(raw);

                if (converted is null) return feature;

                feature = new FeatureDll(converted.Name, converted.RunFilePath, converted.Commands, AppInfoResolver.ShouldEnableNewlyAddedFeature(), converted.Cli, converted.WebApi);

            }

            return feature;
        }
    }
}
