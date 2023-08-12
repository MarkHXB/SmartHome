using Saturn.Mobile.DTO;
using System.Collections.ObjectModel;

namespace Saturn.Mobile.ViewModels
{
    internal class MainPageViewModel : NotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            Features = new ObservableCollection<Feature>();
        }

        public ObservableCollection<Feature> Features { get; set; }
    }
}