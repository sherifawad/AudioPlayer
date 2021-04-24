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

        public App()
        {
            InitializeComponent();

        }
        protected override async void OnStart()
        {
            MainPage = new NavigationPage(new RecordView());
            DependencyService.Register<INavigationService, NavigationService>(); var recordPermission = DependencyService.Get<IRecordPermission>();
            var status = await recordPermission.CheckStatusAsync();
            Device.BeginInvokeOnMainThread(async () => status = await recordPermission.RequestAsync());


        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
