using System.Runtime.InteropServices;

namespace Saturn.Shared
{
    public static class AppInfo
    {
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static readonly string AppName = "Saturn";    
        private static readonly string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
        public static readonly string ConfigFilePath = Path.Combine(Appdata, AppName, "Config.json");
        public static readonly string LogFolderPath = Path.Combine(Appdata, AppName, "Log");  
        public static readonly string FeaturesOutputFolderPath = Path.Combine(Appdata, AppName, "Output");

        // Scheduling
        public static readonly string ScheduledFeaturesFilePath = Path.Combine(Appdata, AppName, "Scheduling.json");

        // Logging
        public static readonly string LogFilePath_CLI = Path.Combine(LogFolderPath, DateTime.Now.ToShortDateString() + "_CLI.log");
        public static readonly string LogFilePath_DAEMON = Path.Combine(LogFolderPath, DateTime.Now.ToShortDateString() + "_DAEMON.log");
        public static readonly string LogFilePath_MENU = Path.Combine(LogFolderPath, DateTime.Now.ToShortDateString() + "_MENU.log");
        public static readonly string LogFilePath_MAUI = Path.Combine(LogFolderPath, DateTime.Now.ToShortDateString() + "_MAUI.log");
        public static readonly string LogFilePath_WEBAPI = Path.Combine(LogFolderPath, DateTime.Now.ToShortDateString() + "_WEBAPI.log");

        // Description
        public static readonly string FeaturesDescriptionFolderPath = Path.Combine(Appdata, AppName, "Features");
        public static readonly string FeatureDescriptionIdentifier = "description.saturn.json";

        #region Constants

        /// <summary>
        /// This delay value represents 1 minute delay on service/daemon run.
        /// </summary>
        public const int DaemonDelayValueInSeconds = 60000;

        public const int MaxCountOfRunAFeaturePerDay = 5;

        #endregion

        public static string GetFeatureFileName(Feature feature)
        {
            _ = feature ?? throw new ArgumentNullException(feature?.Name ?? nameof(feature));

            string fileName = $"{feature.Name}_output_{DateTime.Now.Ticks}.txt";

            return Path.Combine(FeaturesOutputFolderPath, fileName);
        }
    }
}
