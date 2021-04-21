using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace AudioPlayer.Services
{
    public interface IRecordPermission
    {
        Task<PermissionStatus> CheckStatusAsync();
        Task<PermissionStatus> RequestAsync();
    }
}
