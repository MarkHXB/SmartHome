using Saturn.Mobile.DTO;
using Saturn.Mobile.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Saturn.Mobile.ViewModels
{
    internal class FeaturesViewModel : NotifyPropertyChanged
    {
        private readonly IFeatureService _featureService;

        public FeaturesViewModel()
        {
            _featureService = new FeatureService();
            Features = new ObservableCollection<Feature>();
            GetAllCommand = new Command(async () =>
            {
                GeoLocationWatcher geoLocationWatcher = new GeoLocationWatcher();
                await geoLocationWatcher.GetCurrentLocation();

                var features = await _featureService.GetAll();

                if (features is null)
                {
                    return;
                }

                features.ForEach(feature => Features.Add(feature));
            });         
        }
        public ObservableCollection<Feature> Features { get; set; }
        public ICommand GetAllCommand { get; private set; }
    }
}