using Saturn.BL.FeatureUtils;
using System.Text;

namespace Saturn.BL.Persistence
{
    internal static class FeatureOutputFile
    {
        public static async Task Save(Feature feature)
        {
            if (!AppInfoResolver.ShouldSaveFeatureOutputToFile())
            {
                throw new Exception($"{nameof(AppInfoResolver.ShouldSaveFeatureOutputToFile)} is not enabled, but the program tried to save the output of {feature.FeatureName}");
            }

            _ = feature ?? throw new ArgumentNullException(feature?.FeatureName ?? nameof(feature));

            if (feature.Output is null || feature.Output.Count == 0)
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
