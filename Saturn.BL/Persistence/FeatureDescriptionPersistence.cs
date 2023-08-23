using Saturn.Shared;

namespace Saturn.BL.Persistence
{
    public class FeatureDescriptionPersistence
    {
        public static void CopyDescriptionFileToFeaturesFolders(string? sourceDescriptionPath, string? featureName)
        {
            if (string.IsNullOrWhiteSpace(sourceDescriptionPath))
            {
                throw new ArgumentNullException(nameof(sourceDescriptionPath));
            }
            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentNullException(nameof(featureName));
            }

            string descriptionFilePath = Path.Combine(AppInfo.FeaturesDescriptionFolderPath, GetDescriptionFileName(featureName));

            if (IsExists(descriptionFilePath))
            {
                throw new Exception("@BL Feature description is already exists!");
            }

            File.Copy(sourceDescriptionPath, descriptionFilePath, true);
        }

        public static void SaveModifiedDescriptionFiles(IList<Feature> features)
        {

        }

        private static string GetDescriptionFileName(string featureName)
        {
            return string.Join("_", featureName, AppInfo.FeatureDescriptionIdentifier);
        }

        public static bool IsExists(string path)
            => Directory.GetFiles(AppInfo.FeaturesDescriptionFolderPath).Any(desc => Path.GetFileNameWithoutExtension(path).Contains(desc));
    }
}
