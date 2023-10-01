using Saturn.Mobile.DTO;

namespace Saturn.Mobile.Services
{
    public interface IFeatureService
    {
        Task Get(string featureName);
        Task<List<Feature>> GetAll();
        Task Run(string featureName, List<KeyValuePair<string, string>> args);
        Task RunAll();
        Task Enable(string featureName);
        Task Disable(string featureName);
        Task Stop(string featureName);
        Task StopAll();
    }
}
