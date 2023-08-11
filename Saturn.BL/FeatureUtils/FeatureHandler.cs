using Saturn.BL.AppConfig;
using Saturn.BL.Logging;
using Saturn.BL.Persistence;
using Saturn.Persistance;
using System.Diagnostics;

namespace Saturn.BL.FeatureUtils
{
    public class FeatureHandler
    {
        #region Fields

        private static IList<CacheLoadRecord> s_CacheLoadRecords = new List<CacheLoadRecord>()
    {
        new CacheLoadRecord("dll","FeatureDlls"),
        new CacheLoadRecord("exe","FeatureExecutables"),
    };
        public Action<string> LogInformation;
        public Action<string> LogWarning;
        private bool s_Built = false;
        private IList<Feature> m_Features;

        #endregion

        #region Properties

        public bool IsModified { get; private set; }

        #endregion

        #region Constructors

        public FeatureHandler(Action<string> logInformation, Action<string> logWarning)
        {
            _ = logInformation ?? throw new ArgumentNullException(nameof(logInformation));
            _ = logWarning ?? throw new ArgumentNullException(nameof(logWarning));

            LogInformation = logInformation;
            LogWarning = logWarning;
            m_Features = new List<Feature>();
        }

        public FeatureHandler(ILoggerLogicProvider loggerLogicProvider)
        {
            _ = loggerLogicProvider ?? throw new ArgumentNullException(nameof(loggerLogicProvider));

            LogInformation = loggerLogicProvider.LogInformation;
            LogWarning = loggerLogicProvider.LogWarning;
            m_Features = new List<Feature>();
        }

        #endregion

        #region Static methods

        public static async Task<FeatureHandler> BuildAsync(ILoggerLogicProvider loggerLogicProvider, IList<CacheLoadRecord>? cacheLoadRecords = null)
        {
            FeatureHandler featureHandler = new FeatureHandler(loggerLogicProvider);

            ConfigHandler.Build(featureHandler.LogInformation);

            if (AppInfoResolver.UseLoadFromCache())
            {
                var features = await Cache.Load(cacheLoadRecords ?? s_CacheLoadRecords);
                featureHandler.m_Features = features;
            }
            else
            {
                featureHandler.Collect();

                await Cache.Save(featureHandler.m_Features);
            }

            return featureHandler;
        }

        #endregion

        #region Public methods

        public async Task BuildAsync(IList<CacheLoadRecord>? cacheLoadRecords = null)
        {
            if (AppInfoResolver.UseLoadFromCache())
            {
                var features = await Cache.Load(cacheLoadRecords ?? s_CacheLoadRecords);
                m_Features = features;
            }
            else
            {
                Collect();

                await Cache.Save(m_Features);
            }

            s_Built = true;
        }
        public async Task TryToRun(string? featureName = "", string[]? args = null)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentNullException(nameof(featureName) + "\nFeature Name cannot be null!");
            }

            var feature = m_Features.FirstOrDefault(f => f.FeatureName == featureName);

            _ = feature ?? throw new ArgumentNullException(nameof(feature));

            LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} started");

            await feature.Run(args);

            LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} finished");
        }
        public async Task<string> TryToRunReturnOutput(string? featureName = "", string[]? args = null)
        {
            var feature = m_Features.FirstOrDefault(f => f.FeatureName == featureName);

            _ = feature ?? throw new ArgumentNullException(nameof(feature));

            LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} started");

            await feature.Run(args);

            LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} finished");

            return feature.OutputToString();
        }
        public async Task TryToRunAll()
        {
            foreach (var feature in m_Features)
            {
                LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} started");

                await feature.Run();

                LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} finished");
            }
        }
        public void Collect()
        {
            CollectAlternativExecutables();
        }
        public void AddFeature(string? pathToFeature)
        {
            Feature? feature = null;

            // CASE 1: If solution file
            if (FeaturePathIsSolutionFile(pathToFeature ?? string.Empty))
            {
                feature = ConvertSolutionToFeatureExecutableWindows(pathToFeature);
            }
            // CASE 2: If directly exe or dll file
            else
            {
                feature = ParseFeatureByFilePath(pathToFeature, AppInfoResolver.ShouldEnableNewlyAddedFeature());
            }

            RegisterFeature(feature);
        }
        public void EnableFeature(string? featureName)
        {
            var feature = m_Features.FirstOrDefault(f => f.FeatureName == featureName);

            if (feature is not null && feature?.IsEnabled == false)
            {
                feature.Enable();
                IsModified = true;
                LogInformation($" @BL {feature.FeatureName} enabled");
            }
        }
        public void DisableFeature(string? featureName)
        {
            var feature = m_Features.FirstOrDefault(f => f.FeatureName == featureName);

            if (feature is not null && feature?.IsEnabled == true)
            {
                feature.Disable();
                IsModified = true;
                LogInformation($" @BL {feature.FeatureName} disabled");
            }
        }
        public void Stop(string? featureName)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentNullException(nameof(featureName) + "\nFeature Name cannot be null!");
            }

            var feature = m_Features.FirstOrDefault(f => f.FeatureName == featureName);

            _ = feature ?? throw new ArgumentNullException(nameof(feature));

            feature.Stop();

            feature.CancellationRequested -= CancellationRequestedOnFeature;
        }
        public void StopAll()
        {
            foreach (var feature in m_Features)
            {
                feature.Stop();
            }
        }
        public Task ScheduleRun(string? value)
        {
            throw new NotImplementedException();
        }
        public async Task SaveOutputToFile()
        {
            foreach (var feature in m_Features)
            {
                await FeatureOutputFile.Save(feature);
            }
        }
        public async Task<IList<Feature>> GetFeaturesAsync()
        {
            if (!s_Built)
            {
                await BuildAsync();
            }

            return m_Features;
        }
        public IList<Feature> GetFeatures()
        {
            return m_Features;
        }
        public void CancellationRequestedOnFeature(object? sender, EventArgs eventArgs)
        {
            Feature? feature = sender as Feature;

            LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} interrupted.");
        }

        #endregion

        #region Private methods

        private Feature? ConvertSolutionToFeatureExecutableWindows(string? pathToFeature)
        {
            Feature? feature = null;

            if (!FeaturePathIsSolutionFile(pathToFeature ?? string.Empty))
            {
                LogWarning($"@BL Failed to identify solution {pathToFeature}. Try to add feature like this:addfeature ...folder/asd.sln");
                return feature;
            }

            if (AppInfo.IsWindows)
            {
                string pathToBuild = $"C:/tmp/{Path.GetFileNameWithoutExtension(pathToFeature)}";

                string command = $"dotnet publish {pathToFeature} -r win-x64 -o {pathToBuild}";

                ProcessStartInfo config = new ProcessStartInfo()
                {
                    FileName = "dotnet",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };
                Process proc = new Process();
                proc.StartInfo = config;
                proc.Start();
                proc.WaitForExit();

                string[] files = Directory.GetFiles(pathToBuild, $"{Path.GetFileNameWithoutExtension(pathToFeature)}*.exe", SearchOption.AllDirectories);
                string exeFilePath = files.Length > 0 ? files[0] : string.Empty;

                if (string.IsNullOrWhiteSpace(exeFilePath))
                {
                    LogWarning($"@BL Under {pathToBuild} not found any executable file.");
                    return feature;
                }

                if (!Directory.Exists(AppInfo.FeaturesFolderPath))
                {
                    Directory.CreateDirectory(AppInfo.FeaturesFolderPath);
                }

                string destinationExePath = Path.Combine(AppInfo.FeaturesFolderPath, Path.GetFileName(exeFilePath));
                using (var source = new FileStream(exeFilePath, FileMode.Open, FileAccess.Read))
                using (var destination = new FileStream(destinationExePath, FileMode.Create, FileAccess.Write))
                {
                    source.CopyTo(destination);
                }

                feature = ParseFeatureByFilePath(destinationExePath, AppInfoResolver.ShouldEnableNewlyAddedFeature());

                Directory.Delete(pathToBuild, true);
            }
            else
            {
                string pathToBuild = $"/tmp/{Path.GetFileNameWithoutExtension(pathToFeature)}";

                string command = $"dotnet publish {pathToFeature} -r linux-x64 -o {pathToBuild}";

                ProcessStartInfo config = new ProcessStartInfo()
                {
                    FileName = "dotnet",
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                Process proc = new Process();
                proc.StartInfo = config;
                proc.Start();
                proc.WaitForExit();

                string[] files = Directory.GetFiles(pathToBuild, $"{Path.GetFileNameWithoutExtension(pathToFeature)}*.dll", SearchOption.AllDirectories);
                string dllFilePath = files.Length > 0 ? files[0] : string.Empty;

                if (string.IsNullOrWhiteSpace(dllFilePath))
                {
                    LogWarning($"@BL Under {pathToBuild} not found any dll file.");
                    return feature;
                }

                if (!Directory.Exists(AppInfo.FeaturesFolderPath))
                {
                    Directory.CreateDirectory(AppInfo.FeaturesFolderPath);
                }

                string destinationExePath = Path.Combine(AppInfo.FeaturesFolderPath, Path.GetFileName(dllFilePath));
                using (var source = new FileStream(dllFilePath, FileMode.Open, FileAccess.Read))
                using (var destination = new FileStream(destinationExePath, FileMode.Create, FileAccess.Write))
                {
                    source.CopyTo(destination);
                }

                feature = ParseFeatureByFilePath(destinationExePath, AppInfoResolver.ShouldEnableNewlyAddedFeature());

                Directory.Delete(pathToBuild, true);
            }

            return feature;
        }
        private bool FeaturePathIsSolutionFile(string path) => Path.GetExtension(path) == ".sln";
        private void CollectAlternativExecutables()
        {
            string path = AppInfo.FeaturesFolderPath;

            if (!AppInfoResolver.UseAlternativeFeatures())
            {
                return;
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var filePath in Directory.GetFiles(path))
            {
                ParseExecutableToFeature(filePath);
            }
        }
        private void RegisterFeature(Feature? feature)
        {
            _ = feature ?? throw new Exception($" @BL failed to register feature because it is null");

            if (m_Features.Any(f => f.FeatureName == feature.FeatureName))
            {
                LogInformation($" @BL {feature.FeatureName} is already regiestered to <{feature.FeatureResult}> features.");

                return;
            }

            feature.CancellationRequested += CancellationRequestedOnFeature;

            m_Features.Add(feature);

            IsModified = true;

            LogInformation($" @BL [{feature.FeatureName}] <{feature.FeatureResult}> has added to feautres");
        }
        private Feature? ParseFeatureByFilePath(string? pathToFeature, bool enableFeature)
        {
            Feature? feature = null;

            if (string.IsNullOrWhiteSpace(pathToFeature))
            {
                return feature;
            }

            string extension = Path.GetExtension(pathToFeature);
            string featureName = Path.GetFileNameWithoutExtension(pathToFeature);

            if (extension == ".exe")
            {
                feature = new FeatureExecutable(featureName, enableFeature, pathToFeature);
            }
            else if (extension == ".dll")
            {
                feature = new FeatureDll(featureName, enableFeature, pathToFeature);
            }

            return feature;
        }
        private Feature? ParseExecutableToFeature(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            Feature? feature = null;

            if (Path.GetExtension(filePath) == ".exe" && !filePath.Contains("Microsoft"))
            {
                string featureName = Path.GetFileNameWithoutExtension(filePath);

                RegisterFeature(new FeatureExecutable(featureName, AppInfoResolver.ShouldEnableNewlyAddedFeature(), filePath));
            }

            return feature;
        }

        #endregion
    }
}