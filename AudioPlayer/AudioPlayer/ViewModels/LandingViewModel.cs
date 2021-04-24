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

        public bool IsExbanded { get; private set; }
        public Audio RecentMusic { get; private set; }

        public Audio SelectedMusic { get; set; }

        public ICommand SelectionCommand => new Command(async () => await PlayMusic());
        public ICommand NewRecordCommand => new Command(async () => await RecordAsync());
        public ICommand RenameCommand => new Command(async (parameter) => await RenameAsync(parameter));
        public ICommand DeleteCommand => new Command(async (parameter) => await DeleteAsync(parameter));
        public ICommand RefreshCommand => new Command(() => { Task.Delay(100); IsBusy = false; });

        private async Task RenameAsync(object parameter)
        {
            IsExbanded = false;
            if (parameter != null && parameter is Audio audio)
            {
                string result = await App.Current.MainPage.DisplayPromptAsync("Rename", "New name");
                if (!string.IsNullOrWhiteSpace(result) && !string.IsNullOrEmpty(result))
                {
                    try
                    {
                        var parentDir = Path.GetDirectoryName(audio.Url);
                        result.Replace(".wav", string.Empty);
                        var newPath = Path.Combine(parentDir, $"{result.Trim()}.wav");
                        File.Move(audio.Url, newPath);
                        var existAudio = MusicList.FirstOrDefault(x => x == audio);
                        if (existAudio != null)
                        {
                            var indx = MusicList.IndexOf(existAudio);
                            var newAudio = new Audio
                            {
                                Title = result.Trim(),
                                Url = newPath,
                                Artist = audio.Artist,
                                CoverImage = audio.CoverImage,
                                Date = audio.Date,
                                IsRecent = audio.IsRecent
                            };
                            MusicList.Remove(audio);
                            MusicList.Insert(indx, newAudio);

                        }
                    }
                    catch (Exception renameEx)
                    { }
                }

            }
        }


        private async Task DeleteAsync(object parameter)
        {
            IsExbanded = false;

            if (parameter != null && parameter is Audio audio)
            {
                bool answer = await App.Current.MainPage.DisplayAlert("Delete", "Would you like to Delete the Record", "Yes", "No");
                if (answer)
                {
                    try
                    {
                        if (File.Exists(audio.Url))
                        {
                            File.Delete(audio.Url);
                            if (RecentMusic != null && RecentMusic.Url == audio.Url)
                                RecentMusic = null;
                            MusicList.Remove(audio);
                        }
                    }
                    catch (Exception deleteEx)
                    { }
                }
            }
        }

        public override async Task InitializeAsync(object[] navigationData = null)
        {
            IsBusy = true;
            try
            {
                RecentMusic = null;
                if (navigationData != null && navigationData[0] is Audio lastPlayed)
                    RecentMusic = lastPlayed;
                GetMusics();
                await Task.FromResult(true);
            }
            catch (Exception ex)
            { }
            finally
            {
                IsBusy = false;
            }
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

            //var navPars = new object[] { MusicList.Count };
            //await _navigationService.NavigateToAsync<RecordViewModel>(navPars, true);
            await _navigationService.NavigateToAsync<RecordViewModel>(null, true);
        }

        private ObservableCollection<Audio> GetMusics()
        {
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
            return MusicList;
        }
    }
}
