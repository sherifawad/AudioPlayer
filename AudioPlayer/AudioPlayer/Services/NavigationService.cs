using AudioPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AudioPlayer.Services
{
    public class NavigationService : INavigationService
    {
        protected Application CurrentApplication => Application.Current;

        public async Task InitializeAsync()
        {
            await NavigateToAsync<LandingViewModel>();
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
        public Task RemoveLastFromBackStackAsync()
        {
            var mainPage = Application.Current.MainPage as NavigationPage;

            if (mainPage != null)
            {
                mainPage.Navigation.RemovePage(
                    mainPage.Navigation.NavigationStack[mainPage.Navigation.NavigationStack.Count - 2]);
            }

            return Task.FromResult(true);
        }

        public Task RemoveBackStackAsync()
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

            return Task.FromResult(true);
        }

        private async Task InternalNavigateToAsync(Type viewModelType, object[] parameter, bool animated)
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

        private Type GetPageTypeForViewModel(Type viewModelType)
        {
            var viewName = viewModelType.FullName.Replace("Model", string.Empty);
            var viewModelAssemblyName = viewModelType.GetTypeInfo().Assembly.FullName;
            var viewAssemblyName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", viewName, viewModelAssemblyName);
            var viewType = Type.GetType(viewAssemblyName);
            return viewType;
        }

        private Page CreatePage(Type viewModelType, object parameter)
        {
            Type pageType = GetPageTypeForViewModel(viewModelType);
            if (pageType == null)
            {
                throw new Exception($"Cannot locate page type for {viewModelType}");
            }

            Page page = Activator.CreateInstance(pageType) as Page;
            return page;
        }
    }
}
