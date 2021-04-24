using AudioPlayer.Services;
using AudioPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AudioPlayer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SplashScreen : ContentPage
    {
        //private readonly IRecordPermission recordPermission;

        private CancellationTokenSource _cancellation;
        public SplashScreen()
        {
            InitializeComponent();
            //_cancellation = new CancellationTokenSource();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            //CancellationTokenSource cts = _cancellation; // safe copy

            //Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
            //{

            //    if (cts.IsCancellationRequested)
            //        return false;
            //    else
            //    {

            Task.Run(async () =>
            {

                DependencyService.Register<INavigationService, NavigationService>();
                var recordPermission = DependencyService.Get<IRecordPermission>();
                var status = await recordPermission.CheckStatusAsync();
                Device.BeginInvokeOnMainThread(async () =>
                {
                    while (status != PermissionStatus.Granted)
                    {
                        status = await recordPermission.RequestAsync();
                    }
                    await DependencyService.Get<INavigationService>().NavigateToAsync<RecordViewModel>(null, true);
                });
                //Device.BeginInvokeOnMainThread(async () =>
                //{
                //});

            });
            //    }
            //    return true;
            //});

        }
        private void Stop()
        {
            Interlocked.Exchange(ref _cancellation, new CancellationTokenSource()).Cancel();
        }
    }
}