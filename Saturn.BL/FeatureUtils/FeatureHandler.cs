using Saturn.BL.Converters;
using Saturn.BL.Persistence;
using Saturn.Shared;

namespace Saturn.BL.FeatureUtils
{
    public class FeatureHandler
    {
        #region Fields

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

        public static async Task<FeatureHandler> BuildAsync(ILoggerLogicProvider loggerLogicProvider)
        {
            FeatureHandler featureHandler = new FeatureHandler(loggerLogicProvider);

            ConfigHandler.Build(featureHandler.LogInformation);

            featureHandler.Collect();

            return featureHandler;
        }

        #endregion

        #region Public methods

        public async Task BuildAsync()
        {
            await Task.Run(() =>
            {
                Collect();

                s_Built = true;
            });
        }

        public async Task TryToRun(string? featureName = "", string[]? args = null)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentNullException(nameof(featureName) + "\nFeature Name cannot be null!");
            }

            var feature = m_Features.FirstOrDefault(f => f.Name == featureName);

            _ = feature ?? throw new ArgumentNullException(nameof(feature));

            LogInformation($" @BL [{feature.FeatureResult}] {feature.Name} started");

            await feature.Run(args);

            LogInformation($" @BL [{feature.FeatureResult}] {feature.Name} finished");
        }

        public async Task<string> TryToRunReturnOutput(string? featureName = "", string[]? args = null)
        {
            var feature = m_Features.FirstOrDefault(f => f.Name == featureName);

            _ = feature ?? throw new ArgumentNullException(nameof(feature));

            LogInformation($" @BL [{feature.FeatureResult}] {feature.Name} started");

            await feature.Run(args);

            LogInformation($" @BL [{feature.FeatureResult}] {feature.Name} finished");

            return feature.OutputToString();
        }

        public async Task TryToRunAll()
        {
            foreach (var feature in m_Features)
            {
                LogInformation($" @BL [{feature.FeatureResult}] {feature.Name} started");

                await feature.Run();

                LogInformation($" @BL [{feature.FeatureResult}] {feature.Name} finished");
            }
        }

        public void Collect()
        {
            string path = AppInfo.FeaturesDescriptionFolderPath;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var descriptionFiles = Directory.GetFiles(path).Where(p => p.Contains(AppInfo.FeatureDescriptionIdentifier));

            foreach (var filePath in descriptionFiles)
            {
                RegisterFeature(FeatureConverter.Convert(filePath));
            }
        }

        public void AddFeature(string? pathToDescription)
        {
            Feature? feature = null;

            if (ConverterValidity.IsFeatureDescriptionFile(pathToDescription))
            {
                feature = FeatureConverter.Convert(pathToDescription);
                FeatureDescriptionPersistence.CopyDescriptionFileToFeaturesFolders(pathToDescription, feature?.Name);
            }
            else
            {
                LogWarning($"Unrecognized feature description file path: \n{pathToDescription}");

                return;
            }

            RegisterFeature(feature);
        }

        public void EnableFeature(string? featureName)
        {
            var feature = m_Features.FirstOrDefault(f => f.Name == featureName);

            if (feature is not null && feature?.IsEnabled == false)
            {
                feature.Enable();
                IsModified = true;
                LogInformation($" @BL {feature.Name} enabled");
            }
        }

        public void DisableFeature(string? featureName)
        {
            var feature = m_Features.FirstOrDefault(f => f.Name == featureName);

            if (feature is not null && feature?.IsEnabled == true)
            {
                feature.Disable();
                IsModified = true;
                LogInformation($" @BL {feature.Name} disabled");
            }
        }

        public void Stop(string? featureName)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentNullException(nameof(featureName) + "\nFeature Name cannot be null!");
            }

            var feature = m_Features.FirstOrDefault(f => f.Name == featureName);

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

            LogInformation($" @BL [{feature.FeatureResult}] {feature.Name} interrupted.");
        }

        #endregion

        #region Private methods

        private void RegisterFeature(Feature? feature)
        {
            if (feature is null)
            {
                LogWarning($" @BL Failed to register feature by broken description file.");

                return;
            }

            if (string.IsNullOrWhiteSpace(feature.Name))
            {
                LogWarning($" @BL Feature name cannot be empty.");

                return;
            }

            if (m_Features.Any(f => f.Name == feature.Name))
            {
                LogInformation($" @BL {feature.Name} is already regiestered to <{feature.FeatureResult}> features.");

                return;
            }

            feature.CancellationRequested += CancellationRequestedOnFeature;

            m_Features.Add(feature);

            IsModified = true;

            LogInformation($" @BL [{feature.Name}] <{feature.FeatureResult}> has added to feautres");
        }

        public async Task SaveModifiedDescriptionFiles()
        {
            if (!IsModified)
            {
                return;
            }


        }

        #endregion
    }
}