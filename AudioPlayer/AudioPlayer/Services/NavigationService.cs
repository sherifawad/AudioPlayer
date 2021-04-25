using AudioPlayer.ViewModels;
using AudioPlayer.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AudioPlayer.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IRecordPermission _recordPermission;

        protected Application CurrentApplication => Application.Current;

        public NavigationService()
        {
            _recordPermission = DependencyService.Get<IRecordPermission>();
        }
        public Task InitializeAsync()
        {
            return NavigateToAsync<RecordViewModel>();
        }

        public Task NavigateToAsync<TViewModel>(object[] parameter = null, bool animated = false) where TViewModel : BaseViewModel
        {
            return InternalNavigateToAsync(typeof(TViewModel), parameter, animated);
        }

        public Task NavigateToAsync(Type viewModelType, object[] parameter = null, bool animated = false)
        {
            return InternalNavigateToAsync(viewModelType, parameter, animated);
        }

        public async Task NavigateBackAsync()
        {
            if (CurrentApplication.MainPage != null)
            {
                await CurrentApplication.MainPage.Navigation.PopAsync();
            }
        }

        public async Task ClearBackStack()
        {
            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
        public async Task RemoveLastFromBackStackAsync()
        {
            var mainPage = Application.Current.MainPage as NavigationPage;

            if (mainPage != null)
            {
                mainPage.Navigation.RemovePage(
                    mainPage.Navigation.NavigationStack[mainPage.Navigation.NavigationStack.Count - 2]);
            }

            await Task.FromResult(true);
        }

        public async Task RemoveBackStackAsync()
        {
            var mainPage = Application.Current.MainPage as NavigationPage;

            if (mainPage != null)
            {
                for (int i = 0; i < mainPage.Navigation.NavigationStack.Count - 1; i++)
                {
                    var page = mainPage.Navigation.NavigationStack[i];
                    mainPage.Navigation.RemovePage(page);
                }
            }
            await Task.FromResult(true);
        }

        private async Task InternalNavigateToAsync(Type viewModelType, object[] parameter, bool animated)
        {
            try
            {
                Page page = CreatePage(viewModelType, parameter);
                var navigationPage = Application.Current.MainPage as NavigationPage;
                if (navigationPage != null)
                {
                    await navigationPage.PushAsync(page, animated);
                }
                else
                {
                    Application.Current.MainPage = new NavigationPage(page);
                }

                await (page.BindingContext as BaseViewModel).InitializeAsync(parameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private Type GetPageTypeForViewModel(Type viewModelType)
        {
            try
            {
                var viewName = viewModelType.FullName.Replace("Model", string.Empty);
                var viewModelAssemblyName = viewModelType.GetTypeInfo().Assembly.FullName;
                var viewAssemblyName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", viewName, viewModelAssemblyName);
                var viewType = Type.GetType(viewAssemblyName);
                return viewType;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }

        private Page CreatePage(Type viewModelType, object parameter)
        {
            try
            {
                Type pageType = GetPageTypeForViewModel(viewModelType);
                if (pageType == null)
                {
                    throw new Exception($"Cannot locate page type for {viewModelType}");
                }

                Page page = Activator.CreateInstance(pageType) as Page;
                return page;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }

        public async Task<bool> PermissionCheck()
        {
            await _recordPermission.RequestAsync();
            var status = await _recordPermission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
                return (bool)await Application.Current.MainPage.Navigation.ShowPopupAsync(new PermissionsPopup());
            else
                return await Task.FromResult(true);
        }

    }
}
