namespace Saturn.Mobile
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async Task OnCounterClicked(object sender, EventArgs e)
        {
            using(HttpClient client = new HttpClient())
            {
                string json = await client.GetStringAsync("localhost:7160/api/Features/GetAll");
            }
        }
    }
}