using AudioPlayer.Services;
using AudioPlayer.Views;
using System;
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

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<INavigationService, NavigationService>();
            //MainPage = new AppShell();
            //MainPage = new NavigationPage(new LandingPage());
        }
        protected override async void OnStart()
        {
            var recordPermission = DependencyService.Get<IRecordPermission>();
            var status = await recordPermission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await recordPermission.RequestAsync();
                if(status != PermissionStatus.Granted)
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                else
                   await DependencyService.Get<INavigationService>().InitializeAsync();
            }
            else
                await DependencyService.Get<INavigationService>().InitializeAsync();

        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
