using Saturn.BL.FeatureUtils;

namespace Saturn.BL
{
    public static class AppInfo
    {
        public static readonly string AppName = "Saturn";
        private static readonly string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
        public static readonly string CacheFolderPath = Path.Combine(Appdata, AppName, "Cache");
        public static readonly string AlternativeFeatureFolderPath = Path.Combine(Appdata, AppName, "Features");

        public static readonly string LogFolderPath = Path.Combine(Appdata, AppName, "Log");
        public static readonly string LogFilePath_CLI = Path.Combine(LogFolderPath, DateTime.Now.ToShortDateString()+"_CLI.log");
        public static readonly string LogFilePath_API = Path.Combine(LogFolderPath, DateTime.Now.ToShortDateString() + "_API.log");


        public static readonly string FeaturesOutputFolderPath = Path.Combine(Appdata, AppName, "Output");

        /// <summary>
        /// To collect features from Features folder
        /// </summary>
        public static readonly bool UseAlternativeFeatures = true;

        /// <summary>
        /// Start without cache loading
        /// </summary>
        public static readonly bool LoadFromCache = false;

        /// <summary>
        /// Collect and save the features that they are enabled
        /// </summary>
        public static readonly bool ShouldEnableNewlyAddedFeature = true;

        /// <summary>
        /// Save the feature standard output which comes from the process or thread
        /// </summary>
        public static readonly bool SaveFeatureOutputToFile = true;


        public static string GetFeatureFileName(Feature feature)
        {
            _ = feature ?? throw new ArgumentNullException(feature?.FeatureName ?? nameof(feature));

            string fileName = $"{feature.FeatureName}_output_{DateTime.Now.Ticks}.txt";

            return Path.Combine(FeaturesOutputFolderPath, fileName);
        }
    }
}
