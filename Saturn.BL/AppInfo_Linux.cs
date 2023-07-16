namespace Saturn.BL
{
    public static class AppInfo_Linux
    {
        public const string DllFolderPath = @"/usr/share/saturn";

        /// <summary>
        /// To collect features from Features folder
        /// </summary>
        public const bool UseAlternativeFeatures = true;

        /// <summary>
        /// Start without cache loading
        /// </summary>
        public const bool LoadFromCache = false;

        /// <summary>
        /// Collect and save the features that they are enabled
        /// </summary>
        public const bool ShouldEnableNewlyAddedFeature = true;

        /// <summary>
        /// Save the feature standard output which comes from the process or thread
        /// </summary>
        public const bool SaveFeatureOutputToFile = true;
    }
}
