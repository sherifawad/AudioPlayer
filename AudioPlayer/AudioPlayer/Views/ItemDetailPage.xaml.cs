using AudioPlayer.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace AudioPlayer.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}