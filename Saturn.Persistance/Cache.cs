using Newtonsoft.Json;

public class Cache<T>
{
    private readonly string cacheFilePath;

    public Cache(string cacheFilePath)
    {
        this.cacheFilePath = cacheFilePath;
    }

    public bool TryGetValue(string key, out T? value)
    {
        if (File.Exists(cacheFilePath))
        {
            var cacheData = JsonConvert.DeserializeObject<Dictionary<string, T>>(File.ReadAllText(cacheFilePath));

            if(cacheData is null)
            {
                value = default;
                return false;
            }

            if (cacheData.TryGetValue(key, out value))
            {
                return true;
            }
        }

        value = default;
        return false;
    }

    public void SetValue(string key, T value)
    {
        Dictionary<string, T> cacheData = new Dictionary<string, T>();

        if (File.Exists(cacheFilePath))
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, T>>(File.ReadAllText(cacheFilePath));

            if(data is not null)
            {
                cacheData = data;
            }
        }

        cacheData[key] = value;
        File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(cacheData));
    }
}
