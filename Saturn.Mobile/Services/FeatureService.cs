﻿using Newtonsoft.Json;
using Saturn.Mobile.DTO;

namespace Saturn.Mobile.Services
{
    public class FeatureService : BaseRestRequest, IFeatureService
    {
        public Task Disable(string featureName)
        {
            return GetRequest("Disable", new KeyValuePair<string, string>("FeatureName", featureName));
        }

        public Task Enable(string featureName)
        {
            return GetRequest("Enable", new KeyValuePair<string, string>("FeatureName", featureName));
        }

        public Task Get(string featureName)
        {
            return GetRequest("Get", new KeyValuePair<string, string>("FeatureName", featureName));
        }

        public async Task<List<Feature>> GetAll()
        {
            var data = await GetRequest("GetAll");

            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<List<Feature>>(data);
        }

        public Task Run(string featureName)
        {
            return PostRequest("Run", JsonConvert.SerializeObject(featureName));
        }

        public Task RunAll()
        {
            return PostRequest("RunAll");
        }

        public Task Stop(string featureName)
        {
            return GetRequest("Stop", new KeyValuePair<string, string>("FeatureName", featureName));
        }

        public Task StopAll()
        {
            return GetRequest("StopAll");
        }
    }
}
