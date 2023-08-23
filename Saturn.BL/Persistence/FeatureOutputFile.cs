using Saturn.Shared;
using System.Text;

namespace Saturn.BL.Persistence
{
    internal static class FeatureOutputFile
    {
        public static async Task Save(Feature feature)
        {
            if (!AppInfoResolver.ShouldSaveFeatureOutputToFile())
            {
                throw new Exception($"{nameof(AppInfoResolver.ShouldSaveFeatureOutputToFile)} is not enabled, but the program tried to save the output of {feature.Name}");
            }

            _ = feature ?? throw new Exception("@BL Failed to save feature because it's empty!");

            if (!feature.Output.Any())
            {
                return;
            }

            if (!Directory.Exists(AppInfo.FeaturesOutputFolderPath))
            {
                Directory.CreateDirectory(AppInfo.FeaturesOutputFolderPath);
            }

            using (StreamWriter sw = new StreamWriter(path: AppInfo.GetFeatureFileName(feature), append: false, encoding: Encoding.UTF8))
            {
                await sw.WriteLineAsync(feature.OutputToString());
                sw.Close();
            }
        }
    }
}
