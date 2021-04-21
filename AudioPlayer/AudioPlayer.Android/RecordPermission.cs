using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AudioPlayer.Droid;
using AudioPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(RecordPermission))]
namespace AudioPlayer.Droid
{
    public class RecordPermission : Xamarin.Essentials.Permissions.BasePlatformPermission, IRecordPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
        {
            (Android.Manifest.Permission.ReadExternalStorage, true),
            (Android.Manifest.Permission.WriteExternalStorage, true),
            (Android.Manifest.Permission.ModifyAudioSettings, true),
            (Android.Manifest.Permission.RecordAudio, true)
        }.ToArray();
    }
}