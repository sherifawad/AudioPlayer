using AudioPlayer.ViewModels;
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
    public partial class LandingView : ContentPage
    {
        public LandingView()
        {
            InitializeComponent();
        }
        protected override bool OnBackButtonPressed()
        {
            return true;
        }

    }
}