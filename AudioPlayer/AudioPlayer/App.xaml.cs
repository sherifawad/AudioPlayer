using AudioPlayer.IOC;
using AudioPlayer.Services;
using AudioPlayer.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: ExportFont("fa-solid-900.ttf", Alias = "SolidAwesome")]
namespace AudioPlayer
{
    public partial class App : Application
    {

        private readonly INavigationService _navigationService;
        public App()
        {
            InitializeComponent();
            _navigationService = Startup.ServiceProvider.GetService<INavigationService>();
        }
        protected override async void OnStart()
        {
            MainPage = new NavigationPage(new RecordView());
            if (_navigationService != null)
            {
                var permissionsExist = await _navigationService.PermissionCheck();
                if (!permissionsExist)
                    MessagingCenter.Send(MessengerKeys.App, MessengerKeys.Close);
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
