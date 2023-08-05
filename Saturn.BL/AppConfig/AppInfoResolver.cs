namespace Saturn.BL.AppConfig
{
    public static class AppInfoResolver
    {
        public static bool UseLoadFromCache() => AppInfo.IsWindows ? AppInfo_Windows.LoadFromCache : AppInfo_Linux.LoadFromCache;
        public static bool ShouldEnableNewlyAddedFeature() => AppInfo.IsWindows ? AppInfo_Windows.ShouldEnableNewlyAddedFeature : AppInfo_Linux.ShouldEnableNewlyAddedFeature;
        public static bool UseAlternativeFeatures() => AppInfo.IsWindows ? AppInfo_Windows.UseAlternativeFeatures : false;
        public static bool ShouldSaveFeatureOutputToFile() => AppInfo.IsWindows ? AppInfo_Windows.SaveFeatureOutputToFile : AppInfo_Linux.SaveFeatureOutputToFile;
    }
}
