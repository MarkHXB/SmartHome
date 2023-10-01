using Saturn.Mobile.Services;
using Saturn.Mobile.ViewModels;

namespace Saturn.Mobile
{
    public partial class MainPage : ContentPage
    {
        private readonly FeaturesViewModel _featuresViewModel;
        private readonly GeoLocationWatcher _geoLocationWatcher;

        public MainPage()
        {
            InitializeComponent();

            BindingContext = _featuresViewModel = new FeaturesViewModel();
            //_geoLocationWatcher = new GeoLocationWatcher();
            //_geoLocationWatcher.Watch(2500).Start();

            //new Thread(() =>
            //{
            //    Thread.Sleep(10000);
            //    _geoLocationWatcher.CancelRequest();
            //}).Start();
        }
    }
}