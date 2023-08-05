using Saturn.BL.Logging;
using Saturn.BL.Persistence;
using Saturn.Persistance;
using System.Diagnostics;

using System.Configuration;
using Saturn.BL.AppConfig;

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
        public Action<string> m_LogInformation;
        public Action<string> m_LogWarning;
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

            m_LogInformation = logInformation;
            m_LogWarning = logWarning;
            m_Features = new List<Feature>();
        }

        public FeatureHandler(ILoggerLogicProvider loggerLogicProvider)
        {
            _ = loggerLogicProvider ?? throw new ArgumentNullException(nameof(loggerLogicProvider));

            m_LogInformation = loggerLogicProvider.LogInformation;
            m_LogWarning = loggerLogicProvider.LogWarning;
            m_Features = new List<Feature>();
        }

        #endregion

        #region Static methods

        public static async Task<FeatureHandler> BuildAsync(ILoggerLogicProvider loggerLogicProvider, IList<CacheLoadRecord>? cacheLoadRecords = null)
        {
            FeatureHandler featureHandler = new FeatureHandler(loggerLogicProvider);

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

            m_LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} started");

            await feature.Run(args);

            m_LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} finished");
        }
        public async Task<string> TryToRunReturnOutput(string? featureName = "", string[]? args = null)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentNullException(nameof(featureName) + "\nFeature Name cannot be null!");
            }

            var feature = m_Features.FirstOrDefault(f => f.FeatureName == featureName);

            _ = feature ?? throw new ArgumentNullException(nameof(feature));

            m_LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} started");

            await feature.Run(args);

            m_LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} finished");

            return feature.OutputToString();
        }
        public async Task TryToRunAll()
        {
            foreach (var feature in m_Features)
            {
                m_LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} started");

                await feature.Run();

                m_LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} finished");
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
                m_LogInformation($" @BL {feature.FeatureName} enabled");
            }
        }
        public void DisableFeature(string? featureName)
        {
            var feature = m_Features.FirstOrDefault(f => f.FeatureName == featureName);

            if (feature is not null && feature?.IsEnabled == true)
            {
                feature.Disable();
                IsModified = true;
                m_LogInformation($" @BL {feature.FeatureName} disabled");
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

            m_LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} interrupted.");
        }

        #endregion

        #region Private methods

        private Feature? ConvertSolutionToFeatureExecutableWindows(string? pathToFeature)
        {
            Feature? feature = null;

            if (!FeaturePathIsSolutionFile(pathToFeature ?? string.Empty))
            {
                m_LogWarning($"@BL Failed to identify solution {pathToFeature}. Try to add feature like this:addfeature ...folder/asd.sln");
                return feature;
            }

            string command = $"dotnet publish {pathToFeature} -r win-x64";

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

            string output = proc.StandardOutput.ReadToEnd();
            string[] tokens = output.Split(Environment.NewLine);

            string rawExePath = tokens[tokens.Length - 2];
            int indexOfC = rawExePath.IndexOf("C:");
            int indexOfD = rawExePath.IndexOf("D:");
            string executableRootPath = string.Empty;
            if (indexOfC != -1)
            {
                executableRootPath = rawExePath.Substring(indexOfC);
            }
            else if (indexOfD != 1)
            {
                executableRootPath = rawExePath.Substring(indexOfD);
            }

            string[] files = Directory.GetFiles(executableRootPath);
            string exeFilePath = string.Empty;
            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".exe")
                {
                    exeFilePath = file;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(exeFilePath))
            {
                m_LogWarning($"@BL Under {executableRootPath} not found any executable file.");
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
                m_LogInformation($" @BL {feature.FeatureName} is already regiestered to <{feature.FeatureResult}> features.");

                return;
            }

            feature.CancellationRequested += CancellationRequestedOnFeature;

            m_Features.Add(feature);

            IsModified = true;

            m_LogInformation($" @BL [{feature.FeatureName}] <{feature.FeatureResult}> has added to feautres");
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
        private Feature? ParseDllToFeature(string? pathToDll)
        {
            if (string.IsNullOrWhiteSpace(pathToDll))
            {
                return null;
            }

            Feature? feature = null;

            if (Path.GetExtension(pathToDll) == ".dll" && !pathToDll.Contains("Microsoft"))
            {
                string featureName = Path.GetFileNameWithoutExtension(pathToDll);

                RegisterFeature(new FeatureDll(featureName, AppInfoResolver.ShouldEnableNewlyAddedFeature(), pathToDll));
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