using AudioPlayer.IOC;
using AudioPlayer.Services;
using AudioPlayer.ViewModels;
using MediaManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AudioPlayer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecordView : ContentPage
    {

        public RecordView()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            reSwitch.IsToggled = false;
            timeSwitch.IsToggled = false;
            stackTime.IsVisible = false;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                vidPlayer.Repeat = MediaManager.Playback.RepeatMode.One;
                timeSwitch.IsToggled = true;
                stackTime.IsVisible = true;
            }
            else
            {
                vidPlayer.Repeat = MediaManager.Playback.RepeatMode.Off;
                timeSwitch.IsToggled = false;
                stackTime.IsVisible = false;
            }
        }

        private void Switch_Toggled_1(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                tPicker.Time = DateTime.Now.TimeOfDay + TimeSpan.FromSeconds(30);
                Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
                {
                    if (!e.Value)
                        return false;
                    else if (e.Value && DateTime.Now.TimeOfDay >= tPicker.Time)
                    {
                        CrossMediaManager.Current.Stop();
                        return false;
                    }
                    else
                        return true;
                });
            }

        }
    }
}