namespace Saturn.BL.AppConfig
{
    public class AppInfo_Windows
    {
        /// <summary>
        /// To collect features from Features folder
        /// </summary>
        public static bool UseAlternativeFeatures = false;

        /// <summary>
        /// Start without cache loading
        /// </summary>
        public static bool LoadFromCache = false;

        /// <summary>
        /// Collect and save the features that they are enabled
        /// </summary>
        public static bool ShouldEnableNewlyAddedFeature = true;

        /// <summary>
        /// Save the feature standard output which comes from the process or thread
        /// </summary>
        public static bool SaveFeatureOutputToFile = true;
    }
}
