using AudioPlayer.Services;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AudioPlayer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PermissionsPopup : Popup
    {
        private readonly IRecordPermission recordPermission;

        public PermissionsPopup()
        {
            InitializeComponent();
            recordPermission = DependencyService.Get<IRecordPermission>();
        }

        private async void Button_Clicked(object sender, System.EventArgs e)
        {
            await recordPermission.RequestAsync(); 
            var status = await recordPermission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
                Dismiss(false);
            else
                Dismiss(true);
        }

        protected override object GetLightDismissResult()
        {
            var status =  recordPermission.CheckStatusAsync().Result;
            if (status != PermissionStatus.Granted)
                return false;
            else
                return true;
        }
    }
}