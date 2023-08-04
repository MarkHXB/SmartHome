using Newtonsoft.Json;
using Saturn.BL;
using Saturn.BL.FeatureUtils;
using Saturn.BL.Persistence;
using System.Text;

namespace Saturn.Persistance
{
    public static class Cache
    {
        private static string FilePath = Path.Combine(AppInfo.CacheFolderPath, "Features.json");

        public static async Task<List<Feature>> Load(IEnumerable<CacheLoadRecord> cacheLoadRecords)
        {
            if(cacheLoadRecords is null)
            {
                throw new ArgumentNullException(nameof(cacheLoadRecords));
            }

            List<Feature> features = new List<Feature>(); 

            // Load default features
            foreach (var item in cacheLoadRecords)
            {
                if (item.TypeName.ToLower().Contains("dll"))
                {
                    var dlls = await LoadDlls(item.FileName);

                    if (dlls is not null && dlls?.Count() > 0)
                    {
                        features.AddRange(dlls);
                    }
                }

                if (item.TypeName.ToLower().Contains("exe"))
                {
                    var exes = await LoadExes(item.FileName);

                    if (exes is not null && exes?.Count() > 0)
                    {
                        features.AddRange(exes);
                    }
                }
            }

            return features;
        }
        
        private static async Task<IEnumerable<FeatureExecutable>> LoadExes(string specifiedFileName = "")
        {
            string _filePath = FilePath;

            if (!string.IsNullOrWhiteSpace(specifiedFileName))
            {
                _filePath = Path.Combine(AppInfo.CacheFolderPath, specifiedFileName + ".json");
            }

            if (!File.Exists(_filePath))
            {
                using (FileStream file = File.Create(_filePath)) { }
            }

            IEnumerable<FeatureExecutable> entities;

            using (StreamReader streamReader = new StreamReader(_filePath))
            {
                var raw = await streamReader.ReadToEndAsync();

                entities = (IEnumerable<FeatureExecutable>)(JsonConvert.DeserializeObject<IEnumerable<object>>(raw) ?? Array.Empty<FeatureExecutable>());
            }

            return entities;
        }
        private static async Task<IEnumerable<FeatureDll>> LoadDlls(string specifiedFileName = "")
        {
            string _filePath = FilePath;

            if (!string.IsNullOrWhiteSpace(specifiedFileName))
            {
                _filePath = Path.Combine(AppInfo.CacheFolderPath, specifiedFileName + ".json");
            }

            if (!File.Exists(_filePath))
            {
                using (FileStream file = File.Create(_filePath)) { }
            }

            IEnumerable<FeatureDll> entities;

            using (StreamReader streamReader = new StreamReader(_filePath))
            {
                var raw = await streamReader.ReadToEndAsync();

                entities = JsonConvert.DeserializeObject<IEnumerable<FeatureDll>>(raw) ?? Array.Empty<FeatureDll>();
            }

            return entities;
        }

        public static async Task Save(IEnumerable<Feature> entities, string specifiedFileName = "")
        {
            string _filePath = FilePath;

            if (!string.IsNullOrWhiteSpace(specifiedFileName))
            {
                _filePath = Path.Combine(AppInfo.CacheFolderPath, specifiedFileName + ".json");
            }

            _ = entities ?? throw new ArgumentNullException(nameof(entities));

            if (!Directory.Exists(AppInfo.CacheFolderPath))
            {
                Directory.CreateDirectory(AppInfo.CacheFolderPath);
            }

            var dlls = new List<FeatureDll>();
            var exes = new List<FeatureExecutable>();

            foreach (var entity in entities)
            {
                if(entity is FeatureDll)
                {
                    dlls.Add(entity as FeatureDll); 
                }
                if (entity is FeatureExecutable)
                {
                    exes.Add(entity as FeatureExecutable);
                }
            }

            if(dlls.Count > 0)
            {
                string data = JsonConvert.SerializeObject(dlls, Formatting.Indented);

                ReGenerateFilePathPlural(nameof(FeatureDll));

                _filePath = FilePath;

                using (StreamWriter writer = new StreamWriter(_filePath, append: false, encoding: Encoding.UTF8))
                {
                    await writer.WriteLineAsync(data);

                    writer.Close();
                }
            }
            if (exes.Count > 0)
            {
                string data = JsonConvert.SerializeObject(exes, Formatting.Indented);

                ReGenerateFilePathPlural(nameof(FeatureExecutable));

                _filePath = FilePath;

                using (StreamWriter writer = new StreamWriter(_filePath, append: false, encoding: Encoding.UTF8))
                {
                    await writer.WriteLineAsync(data);

                    writer.Close();
                }
            }
        }

        private static void ReGenerateFilePath(string nameOfType)
        {
            FilePath = Path.Combine(AppInfo.CacheFolderPath, nameOfType+".json");
        }
        private static void ReGenerateFilePathPlural(string nameOfType)
        {
            string not = nameOfType.EndsWith("s") ? nameOfType : nameOfType + "s";

            FilePath = Path.Combine(AppInfo.CacheFolderPath, not + ".json");
        }
    }
}
