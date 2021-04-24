using AudioPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AudioPlayer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlayerView : ContentPage
    {
        private CancellationTokenSource animateTimerCancellationTokenSource;
        public PlayerView()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<string, bool>(MessengerKeys.App, MessengerKeys.Play, OnPlay);
        }
        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void OnPlay(string app, bool isPlaying)
        {
            if (app.Equals(MessengerKeys.App))
            {
                RotateCover(isPlaying);
            }
        }

        private void RotateCover(bool isPlaying)
        {
            if (isPlaying)
            {
                StartCoverAnimation(new CancellationTokenSource());
            }
            else
            {
                ViewExtensions.CancelAnimations(CoverImage);

                if (animateTimerCancellationTokenSource != null)
                {
                    animateTimerCancellationTokenSource.Cancel();
                }
            }
        }

        void StartCoverAnimation(CancellationTokenSource tokenSource)
        {
            try
            {
                animateTimerCancellationTokenSource = tokenSource;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (!animateTimerCancellationTokenSource.IsCancellationRequested)
                    {
                        await CoverImage.RelRotateTo(-360, AppSettings.CoverAnimationDuration, Easing.Linear);

                        StartCoverAnimation(animateTimerCancellationTokenSource);
                    }
                });
            }
            catch (TaskCanceledException ex)
            {
                Debug.Fail(ex.Message);
            }
        }
    }
}