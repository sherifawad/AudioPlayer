using AudioPlayer.ViewModels;
using AudioPlayer.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AudioPlayer
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
            Routing.RegisterRoute(nameof(PlayerPage), typeof(PlayerPage));
        }

    }
}
