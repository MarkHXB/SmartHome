namespace Saturn.Shared
{
    public static class AppInfoResolver
    { 
        /*
         * Template:
         * Should -> bool
         * Get -> other values
         */

        public static bool ShouldEnableNewlyAddedFeature() => AppInfo.IsWindows ? AppInfo_Windows.ShouldEnableNewlyAddedFeature : AppInfo_Linux.ShouldEnableNewlyAddedFeature;
        public static bool ShouldSaveFeatureOutputToFile() => AppInfo.IsWindows ? AppInfo_Windows.SaveFeatureOutputToFile : AppInfo_Linux.SaveFeatureOutputToFile;

    }
}
