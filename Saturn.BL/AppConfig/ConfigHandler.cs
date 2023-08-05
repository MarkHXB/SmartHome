using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace Saturn.BL.AppConfig
{
    public class ConfigHandler
    {
        private static bool _initialized = false;

        public static void Build()
        {
            if (_initialized) return;

            if (!IsConfigFileValid())
            {
                RestoreConfigFile();
            }

            Load().GetAwaiter().GetResult();

            _initialized = true;
        }

        public static async Task Load()
        {
            string raw = await File.ReadAllTextAsync(AppInfo.ConfigFilePath);

            // FOR WINDOWS
            if (AppInfo.IsWindows)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);
                foreach (var original in typeof(AppInfo_Windows).GetRuntimeFields())
                {
                    foreach (var latest in data)
                    {
                        if (original.Name.Equals(latest.Key))
                        {
                            original.SetValue(latest, latest.Value);
                        }
                    }
                }
            }
            // FOR LINUX
            else
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);
                foreach (var original in typeof(AppInfo_Linux).GetRuntimeFields())
                {
                    foreach (var latest in data)
                    {
                        if (original.Name.Equals(latest.Key))
                        {
                            original.SetValue(latest, latest.Value);
                        }
                    }
                }
            }
        }

        public static async Task Save()
        {
            string data = "[]";

            var fieldDictionary = new Dictionary<string, object>();

            if (AppInfo.IsWindows)
            {
                foreach (var field in typeof(AppInfo_Windows).GetRuntimeFields())
                {
                    fieldDictionary.Add(field.Name, field.GetValue(field));
                }

                data = JsonConvert.SerializeObject(fieldDictionary);
            }
            else
            {
                foreach (var field in typeof(AppInfo_Linux).GetRuntimeFields())
                {
                    fieldDictionary.Add(field.Name, field.GetValue(field));
                }

                data = JsonConvert.SerializeObject(typeof(AppInfo_Linux).GetRuntimeFields());
            }

            await File.WriteAllTextAsync(AppInfo.ConfigFilePath, data, Encoding.UTF8);
        }

        private static bool IsConfigFileValid()
        {
            if (!File.Exists(AppInfo.ConfigFilePath))
            {
                return false;
            }
            if (!File.ReadAllText(AppInfo.ConfigFilePath).Any())
            {
                return false;
            }

            return true;
        }

        private static void RestoreConfigFile()
        {
            if (!File.Exists(AppInfo.ConfigFilePath))
            {
                File.Create(AppInfo.ConfigFilePath).Close();
            }

            Save().GetAwaiter().GetResult();
        }
    }
}
