namespace Saturn.BL.AppConfig
{
    public static class AppInfoResolver
    {
        public static bool UseLoadFromCache() => ShouldLoadFromCache();
        public static bool ShouldEnableNewlyAddedFeature() => AppInfo.IsWindows ? AppInfo_Windows.ShouldEnableNewlyAddedFeature : AppInfo_Linux.ShouldEnableNewlyAddedFeature;
        public static bool UseAlternativeFeatures() => AppInfo.IsWindows ? AppInfo_Windows.UseAlternativeFeatures : false;
        public static bool ShouldSaveFeatureOutputToFile() => AppInfo.IsWindows ? AppInfo_Windows.SaveFeatureOutputToFile : AppInfo_Linux.SaveFeatureOutputToFile;

        private static bool ShouldLoadFromCache()
        {
            if (AppInfo.IsWindows)
            {
                if (Directory.Exists(AppInfo.FeaturesFolderPath) && Directory.GetLastWriteTime(AppInfo.FeaturesFolderPath) == AppInfo_Windows.LastAccessTimeOfFeatureCache)
                {
                    AppInfo_Windows.LoadFromCache = true;
                    return true;
                }

                AppInfo_Windows.LastAccessTimeOfFeatureCache = Directory.GetLastWriteTime(AppInfo.FeaturesFolderPath);
                AppInfo_Windows.LoadFromCache = false;

                return false;
            }
            else
            {
                if (Directory.Exists(AppInfo.FeaturesFolderPath) && Directory.GetLastWriteTime(AppInfo.FeaturesFolderPath) == AppInfo_Linux.LastAccessTimeOfFeatureCache)
                {
                    AppInfo_Linux.LoadFromCache = true;
                    return true;
                }

                AppInfo_Linux.LastAccessTimeOfFeatureCache = Directory.GetLastWriteTime(AppInfo.FeaturesFolderPath);
                AppInfo_Linux.LoadFromCache = false;

                return false;
            }
        }
    }
}
