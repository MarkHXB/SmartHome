using System.Diagnostics;

namespace Saturn.Mobile.Services
{
    public class GeoLocationWatcher
    {
        private CancellationTokenSource _cancelTokenSource;
        private bool _isCheckingLocation;
        private readonly IFeatureService _featureService;

        public GeoLocationWatcher()
        {
            _featureService = new FeatureService();
        }

        public Task Watch(int millisecondsTimeout)
        {
            return new Task(async () =>
            {
                while (_cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
                {
                    await GetCurrentLocation();

                    Thread.Sleep(millisecondsTimeout);
                }
            });
        }

        public async Task GetCurrentLocation()
        {
            try
            {
                _isCheckingLocation = true;

                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));

                _cancelTokenSource = new CancellationTokenSource();

                Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

                if (location != null)
                {
                    var loc = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("Latitude", location.Latitude.ToString()),
                        new KeyValuePair<string, string>("Longitude", location.Longitude.ToString()),
                    };

                    await _featureService.Run("locator", loc);

                    Trace.WriteLine(loc);
                }
            }
            // Catch one of the following exceptions:
            //   FeatureNotSupportedException
            //   FeatureNotEnabledException
            //   PermissionException
            catch (Exception ex)
            {
                // Unable to get location
            }
            finally
            {
                _isCheckingLocation = false;
            }
        }

        public void CancelRequest()
        {
            if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
                _cancelTokenSource.Cancel();
        }
    }
}
