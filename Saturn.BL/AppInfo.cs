using Saturn.BL.FeatureUtils;
using System.Runtime.InteropServices;

namespace Saturn.BL
{  
    public static class AppInfo 
    {
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static readonly string AppName = "Saturn";
        private static readonly string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
        public static readonly string CacheFolderPath = Path.Combine(Appdata, AppName, "Cache");
        public static readonly string AlternativeFeatureFolderPath = Path.Combine(Appdata, AppName, "Features");
        public static readonly string LogFolderPath = Path.Combine(Appdata, AppName, "Log");
        public static readonly string LogFilePath_CLI = Path.Combine(LogFolderPath, DateTime.Now.ToShortDateString()+"_CLI.log");
        public static readonly string LogFilePath_API = Path.Combine(LogFolderPath, DateTime.Now.ToShortDateString() + "_API.log");
        public static readonly string FeaturesOutputFolderPath = Path.Combine(Appdata, AppName, "Output");

        public static string GetFeatureFileName(Feature feature)
        {
            _ = feature ?? throw new ArgumentNullException(feature?.FeatureName ?? nameof(feature));

            string fileName = $"{feature.FeatureName}_output_{DateTime.Now.Ticks}.txt";

            return Path.Combine(FeaturesOutputFolderPath, fileName);
        }
    }
}
