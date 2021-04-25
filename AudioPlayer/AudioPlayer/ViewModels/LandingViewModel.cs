using AudioPlayer.Models;
using AudioPlayer.Services;
using AudioPlayer.Views;
using MediaManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AudioPlayer.ViewModels
{
    public class LandingViewModel : BaseViewModel
    {
        #region Private Properties
        private bool isExbanded;
        private Audio recentMusic;
        private Audio selectedMusic;
        private readonly INavigationService _navigationService;
        #endregion

        #region public Properties
        public ObservableCollection<Audio> MusicList { get; }
        public bool IsExbanded
        {
            get => isExbanded;
            private set => SetProperty(ref isExbanded, value);
        }
        public Audio RecentMusic
        {
            get => recentMusic;
            private set => SetProperty(ref recentMusic, value);
        }

        public Audio SelectedMusic
        {
            get => selectedMusic;
            set => SetProperty(ref selectedMusic, value);
        }

        #endregion

        #region Public Commands
        public IAsyncCommand SelectionCommand { get; }
        public IAsyncCommand NewRecordCommand { get; }
        public IAsyncCommand<Audio> RenameCommand { get; }
        public IAsyncCommand<Audio> DeleteCommand { get; }
        public ICommand RefreshCommand => new Command(() => { Task.Delay(100); IsBusy = false; });
        #endregion

        #region Default Constructor
        public LandingViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            MusicList = new ObservableCollection<Audio>();
            SelectionCommand = new AsyncCommand(PlayMusic, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            NewRecordCommand = new AsyncCommand(RecordAsync, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            RenameCommand = new AsyncCommand<Audio>(RenameAsync, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            DeleteCommand = new AsyncCommand<Audio>(DeleteAsync, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
        }
        #endregion

        #region Commands Methods

        private async Task RenameAsync(Audio audio)
        {
            IsExbanded = false;
            string result = await App.Current.MainPage.DisplayPromptAsync("Rename", "New name");
            if (!string.IsNullOrWhiteSpace(result) && !string.IsNullOrEmpty(result))
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
        }
        private async Task DeleteAsync(Audio audio)
        {
            IsExbanded = false;
            bool answer = await App.Current.MainPage.DisplayAlert("Delete", "Would you like to Delete the Record", "Yes", "No");
            if (answer)
            {
                if (File.Exists(audio.Url))
                {
                    File.Delete(audio.Url);
                    if (RecentMusic != null && RecentMusic.Url == audio.Url)
                        RecentMusic = null;
                    MusicList.Remove(audio);
                }
            }
        }
        private async Task PlayMusic()
        {
            if (SelectedMusic != null && !IsBusy)
            {
                var navPars = new object[] { SelectedMusic, MusicList.ToList() };
                await _navigationService.NavigateToAsync<PlayerViewModel>(navPars, true);
            }
        }
        private async Task RecordAsync() => await _navigationService.NavigateToAsync<RecordViewModel>(null, true);

        #endregion

        #region Overrided Methods
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
            {
                Debug.Fail(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Private Methods
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
            {
                Debug.Fail(ex.Message);
            }
            return MusicList;
        }

        #endregion
    }
}
