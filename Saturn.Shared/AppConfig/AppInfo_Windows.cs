namespace Saturn.Shared
{
    public class AppInfo_Windows
    {
        #region Don't change visibility or writability of these fields

        /// <summary>
        /// Collect and save the features that they are enabled
        /// </summary>
        public static bool ShouldEnableNewlyAddedFeature = true;

        /// <summary>
        /// Save the feature standard output which comes from the process or thread
        /// </summary>
        public static bool SaveFeatureOutputToFile = true;

        /// <summary>
        /// This is for automatic cache load, if not change then cache load, if changed then collect
        /// </summary>
        public static DateTime LastAccessTimeOfFeatureCache = DateTime.MinValue;

        #endregion
    }
}
