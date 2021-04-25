using AudioPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AudioPlayer.Services
{
    public interface INavigationService
    {
        Task InitializeAsync();

        Task NavigateToAsync<TViewModel>(object[] parameter = null, bool animated = false) where TViewModel : BaseViewModel;

        Task ClearBackStack();

        Task NavigateToAsync(Type viewModelType, object[] parameter = null, bool animated = false);

        Task NavigateBackAsync();

        Task RemoveLastFromBackStackAsync();
        Task<bool> PermissionCheck();
    }
}
