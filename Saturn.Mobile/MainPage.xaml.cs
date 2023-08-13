using Saturn.Mobile.ViewModels;

namespace Saturn.Mobile
{
    public partial class MainPage : ContentPage
    {
        private readonly FeaturesViewModel _featuresViewModel;

        public MainPage()
        {
            InitializeComponent();

            BindingContext = _featuresViewModel = new FeaturesViewModel();
        }
    }
}