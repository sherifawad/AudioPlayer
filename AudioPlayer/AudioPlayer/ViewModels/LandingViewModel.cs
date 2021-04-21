using AudioPlayer.Models;
using AudioPlayer.Views;
using MediaManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AudioPlayer.ViewModels
{
    public class LandingViewModel : BaseViewModel
    {
        public LandingViewModel()
        {
            MusicList = new ObservableCollection<Audio>();
        }

        public ObservableCollection<Audio> MusicList { get; private set; }

        public Audio RecentMusic { get; private set; }

        public Audio SelectedMusic { get; set; }

        public ICommand SelectionCommand => new Command(async () => await PlayMusic());
        public ICommand NewRecordCommand => new Command(async () => await RecordAsync());

        public override async Task InitializeAsync(object[] navigationData = null)
        {
            try
            {
                await CrossMediaManager.Current.Stop();
                RecentMusic = null;
                if (navigationData != null && navigationData[0] is Audio lastPlayed)
                    RecentMusic = lastPlayed;
                GetMusics();
                await Task.FromResult(true);
            }
            catch (Exception ex)
            {}
        }

        private async Task PlayMusic()
        {
            if (SelectedMusic != null && !IsBusy)
            {
                var navPars = new object[] { SelectedMusic, MusicList.ToList() };
                //var viewModel = new PlayerViewModel(SelectedMusic, MusicList);
                //var playerPage = new PlayerPage { BindingContext = viewModel };
                //var navigation = Application.Current.MainPage as NavigationPage;
                await _navigationService.NavigateToAsync<PlayerViewModel>(navPars, true);

                //await Shell.Current.GoToAsync(nameof(playerPage), true);
            }
        }
        private async Task RecordAsync()
        {

            var navPars = new object[] { MusicList.Count };
            await _navigationService.NavigateToAsync<RecordViewModel>(navPars, true);
        }

        private ObservableCollection<Audio> GetMusics()
        {
            IsBusy = true;
            try
            {
                string wavPattern = ".wav";

                DirectoryInfo di = new DirectoryInfo(FileSystem.AppDataDirectory);
                FileSystemInfo[] files = di.GetFileSystemInfos();
                var orderedFiles = files.Where(f => f.Name.EndsWith(wavPattern))
                                        .OrderByDescending(f => f.CreationTime)
                                        .ToList();

                if (orderedFiles != null && orderedFiles.Count > 0)
                {
                    MusicList.Clear();
                    for (int i = 0; i < orderedFiles.Count; i++)
                    {
                        var audio = new Audio
                        {
                            Title = Path.GetFileNameWithoutExtension(orderedFiles[i].Name),
                            Url = orderedFiles[i].FullName,
                            Date = orderedFiles[i].CreationTime
                        };

                        if (RecentMusic == null && i == 0)
                            RecentMusic = audio;

                        MusicList.Add(audio);
                    }
                }
            }
            catch (Exception ex)
            { }
            finally
            {
                IsBusy = false;
            }
            return MusicList;
        }
    }
}
