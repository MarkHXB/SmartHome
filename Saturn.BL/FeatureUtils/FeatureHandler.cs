#define DLL_COLLECTIONS_IS_UNDEFINED

using Saturn.BL.Logging;
using Saturn.BL.Persistence;
using Saturn.Persistance;
using Serilog.Core;
using System.Diagnostics;
using System.Reflection;

namespace Saturn.BL.FeatureUtils
{
    public class FeatureHandler
    {
        #region Fields

        private readonly string DllFolderPath = @"C:\Shared\Release";
        private readonly string ExecutablesFolderPath = @"C:\Shared\Release";
        private static IList<CacheLoadRecord> s_CacheLoadRecords = new List<CacheLoadRecord>()
    {
        new CacheLoadRecord("dll","FeatureDlls"),
        new CacheLoadRecord("exe","FeatureExecutables"),
    };

        public Action<string> LogInformation;
        public Action<string> LogWarning;

        private bool Built = false;

        private IList<Feature> Features { get; set; }   

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
            Features = new List<Feature>();
        }

        public FeatureHandler(ILoggerLogicProvider loggerLogicProvider)
        {
            _ = loggerLogicProvider ?? throw new ArgumentNullException(nameof(loggerLogicProvider));

            LogInformation = loggerLogicProvider.LogInformation;
            LogWarning = loggerLogicProvider.LogWarning;
            Features = new List<Feature>();
        }

        #endregion

        #region Static methods

        public static async Task<FeatureHandler> BuildAsync(ILoggerLogicProvider loggerLogicProvider, IList<CacheLoadRecord>? cacheLoadRecords = null)
        {
            FeatureHandler featureHandler = new FeatureHandler(loggerLogicProvider);

            if (AppInfo.LoadFromCache)
            {
                var features = await Cache.Load(cacheLoadRecords ?? s_CacheLoadRecords);
                featureHandler.Features = features;
            }
            else
            {
                await featureHandler.Collect();

                await Cache.Save(featureHandler.Features);
            }

            return featureHandler;
        }

        #endregion

        #region Public methods

        public async Task BuildAsync(IList<CacheLoadRecord>? cacheLoadRecords = null)
        {
            if (AppInfo.LoadFromCache)
            {
                var features = await Cache.Load(cacheLoadRecords ?? s_CacheLoadRecords);
                Features = features;
            }
            else
            {
                await Collect();

                await Cache.Save(Features);
            }

            Built = true;
        }
        public async Task TryToRun(string? featureName = "", string[]? args = null)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentNullException(nameof(featureName) + "\nFeature Name cannot be null!");
            }

            var feature = Features.FirstOrDefault(f => f.FeatureName == featureName);

            _ = feature ?? throw new ArgumentNullException(nameof(feature));

            LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} started");

            await feature.Run(args);

            LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} finished");
        }
        public async Task<string> TryToRunReturnOutput(string? featureName = "", string[]? args = null)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentNullException(nameof(featureName) + "\nFeature Name cannot be null!");
            }

            var feature = Features.FirstOrDefault(f => f.FeatureName == featureName);

            _ = feature ?? throw new ArgumentNullException(nameof(feature));

            LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} started");

            await feature.Run(args);

            LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} finished");

            return feature.OutputToString();
        }
        public async Task TryToRunAll()
        {
            foreach (var feature in Features)
            {
                LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} started");

                await feature.Run();

                LogInformation($" @BL [{feature.FeatureResult}] {feature.FeatureName} finished");
            }
        }
        public async Task Collect()
        {
            CollectAlternativExecutables();

            await CollectExecutables();

#if DLL_COLLECTIONS_IS_DEFINED
            await CollectDlls();
#endif
        }
        public void AddFeature(string? pathToFeature)
        {
            Feature? feature = null;

            // CASE 1: If solution file
            if (FeaturePathIsSolutionFile(pathToFeature ?? string.Empty))
            {
                feature = ConvertSolutionToFeatureExecutable(pathToFeature);
            }
            // CASE 2: If directly exe or dll file
            else
            {
                feature = ParseFeatureByFilePath(pathToFeature, AppInfo.ShouldEnableNewlyAddedFeature);
            }

            RegisterFeature(feature);
        }
        public void EnableFeature(string? featureName)
        {
            var feature = Features.FirstOrDefault(f => f.FeatureName == featureName);

            if (feature is not null && feature?.IsEnabled == false)
            {
                feature.Enable();
                IsModified = true;
                LogInformation($" @BL {feature.FeatureName} enabled");
            }
        }
        public void DisableFeature(string? featureName)
        {
            var feature = Features.FirstOrDefault(f => f.FeatureName == featureName);

            if (feature is not null && feature?.IsEnabled == true)
            {
                feature.Disable();
                IsModified = true;
                LogInformation($" @BL {feature.FeatureName} disabled");
            }
        }
        public Task ScheduleRun(string? value)
        {
            throw new NotImplementedException();
        }
        public async Task SaveOutputToFile()
        {
            foreach (var feature in Features)
            {
                await FeatureOutputFile.Save(feature);
            }
        }

        public async Task<IList<Feature>> GetFeaturesAsync()
        {
            if (!Built)
            {
                await BuildAsync();
            }

            return Features;
        }
        public IList<Feature> GetFeatures()
        {
            return Features;
        }

        #endregion

        #region Private methods

        private Feature? ConvertSolutionToFeatureExecutable(string? pathToFeature)
        {
            Feature? feature = null;

            if (!FeaturePathIsSolutionFile(pathToFeature ?? string.Empty))
            {
                LogWarning($"@BL Failed to identify solution {pathToFeature}. Try to add feature like this:addfeature ...folder/asd.sln");
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
                LogWarning($"@BL Under {executableRootPath} not found any executable file.");
                return feature;
            }

            if (!Directory.Exists(AppInfo.AlternativeFeatureFolderPath))
            {
                Directory.CreateDirectory(AppInfo.AlternativeFeatureFolderPath);
            }

            string destinationExePath = Path.Combine(AppInfo.AlternativeFeatureFolderPath, Path.GetFileName(exeFilePath));
            using (var source = new FileStream(exeFilePath, FileMode.Open, FileAccess.Read))
            using (var destination = new FileStream(destinationExePath, FileMode.Create, FileAccess.Write))
            {
                source.CopyTo(destination);
            }

            feature = ParseFeatureByFilePath(destinationExePath, AppInfo.ShouldEnableNewlyAddedFeature);

            return feature;
        }
        private bool FeaturePathIsSolutionFile(string path) => Path.GetExtension(path) == ".sln";
        private void CollectAlternativExecutables()
        {
            string path = AppInfo.AlternativeFeatureFolderPath;

            if (!AppInfo.UseAlternativeFeatures)
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
        private async Task CollectDlls()
        {
            if (!Directory.Exists(DllFolderPath))
            {
                throw new DllNotFoundException(nameof(DllFolderPath));
            }

            await Task.Run(() =>
            {
                string[] folderPaths = Directory.GetDirectories(DllFolderPath);

                foreach (var dotnetVersionFolderPath in folderPaths)
                {
                    string[] files = Directory.GetFiles(dotnetVersionFolderPath);

                    foreach (var filePath in files)
                    {
                        ParseDllToFeature(filePath);
                    }
                }
            });
        }
        private async Task CollectExecutables()
        {
            if (!Directory.Exists(ExecutablesFolderPath))
            {
                throw new ExecutableFolderNotFound(nameof(ExecutablesFolderPath));
            }

            await Task.Run(() =>
            {
                string[] folderPaths = Directory.GetDirectories(ExecutablesFolderPath);

                foreach (var dotnetVersionFolderPath in folderPaths)
                {
                    string[] files = Directory.GetFiles(dotnetVersionFolderPath);

                    foreach (var filePath in files)
                    {
                        ParseExecutableToFeature(filePath);
                    }
                }
            });
        }
        private void RegisterFeature(Feature? feature)
        {
            _ = feature ?? throw new Exception($" @BL failed to register feature because it is null");

            if (Features.Any(f => f.FeatureName == feature.FeatureName))
            {
                LogInformation($" @BL {feature.FeatureName} is already regiestered to <{feature.FeatureResult}> features.");

                return;
            }

            Features.Add(feature);

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
                feature = new FeatureExecutable(featureName, pathToFeature, enableFeature);
            }
            else if (extension == ".dll")
            {
                feature = new FeatureDll(featureName, pathToFeature, enableFeature);
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
                var dll = Assembly.LoadFile(pathToDll);

                var program = dll.GetTypes().FirstOrDefault(t => t.Name == "Program");

                if (program is null)
                {
                    return feature;
                }

                var method = program.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

                if (method is null)
                {
                    return feature;
                }

                RegisterFeature(new FeatureDll(dll.ManifestModule.Name, dll.ManifestModule.Name, program, AppInfo.ShouldEnableNewlyAddedFeature));
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

                RegisterFeature(new FeatureExecutable(featureName, filePath, AppInfo.ShouldEnableNewlyAddedFeature));
            }

            return feature;
        }

        #endregion
    }
}