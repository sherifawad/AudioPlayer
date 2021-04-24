using AudioPlayer.Extensions;
using AudioPlayer.Models;
using MediaManager;
using Plugin.AudioRecorder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AudioPlayer.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        #region private Properties
        private List<Audio> MusicList;
        private const string REPEATONEICON = "\uf021";
        private const string REPEATALLICON = "\uf079";
        double maximum = 100f;
        private bool isPlaying;
        private bool repeatOne;
        private bool repeatAll;
        private int repeatMode;
        private bool isPlayTillActive;
        private double progress;
        private Audio selectedMusic;
        private bool noShuffle;
        private bool noRepeat;
        private string repeatIcon;
        private TimeSpan duration;
        private TimeSpan position;
        private bool isRepeatEnabled;
        private TimeSpan selectedTime;
        #endregion

        #region public Properties
        public double Progress
        {
            get => progress;
            set => SetProperty(ref progress, value);
        }

        public Audio SelectedMusic
        {
            get => selectedMusic;
            set => SetProperty(ref selectedMusic, value);
        }

        public bool NoShuffle
        {
            get => noShuffle;
            set => SetProperty(ref noShuffle, value);
        }
        public bool NoRepeat
        {
            get => noRepeat;
            set => SetProperty(ref noRepeat, value);
        }
        public string RepeatIcon
        {
            get => repeatIcon;
            set => SetProperty(ref repeatIcon, value);
        }
        public TimeSpan Duration
        {
            get => duration;
            set => SetProperty(ref duration, value);
        }
        public TimeSpan Position
        {
            get => position;
            set => SetProperty(ref position, value);
        }
        public bool IsRepeatEnabled
        {
            get => isRepeatEnabled;
            private set => SetProperty(ref isRepeatEnabled, value);
        }
        public bool IsPlayTillActive
        {
            get => isPlayTillActive;
            set
            {
                SetProperty(ref isPlayTillActive, value);
                if (value)
                    SelectedTime = DateTime.Now.TimeOfDay + TimeSpan.FromMinutes(2);
            }
        }
        public TimeSpan SelectedTime 
        {
            get => selectedTime; 
            set => SetProperty(ref selectedTime, value);
        }

        public double Maximum
        {
            get { return maximum; }
            set
            {
                if (value > 0)
                {
                    SetProperty(ref maximum, value);
                }
            }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                SetProperty(ref isPlaying, value);
                OnPropertyChanged(nameof(PlayIcon));
                MessagingCenter.Send(MessengerKeys.App, MessengerKeys.Play, IsPlaying);
            }
        }

        public string PlayIcon { get => isPlaying ? "pause.png" : "play.png"; }

        #endregion

        #region Commands Properties

        public IAsyncCommand RepeatCommand { get; }
        public IAsyncValueCommand ShuffleCommand { get; }
        public IAsyncCommand PlayCommand { get; }
        public IAsyncCommand<string> ChangeCommand { get; }
        public IAsyncValueCommand BackCommand { get; }
        public ICommand ShareCommand { get; }
        #endregion

        #region Default Constructor
        public PlayerViewModel()
        {
            RepeatCommand = new AsyncCommand(Repeat, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            PlayCommand = new AsyncCommand(Play, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            ChangeCommand = new AsyncCommand<string>(ChangeMusic, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            BackCommand = new AsyncValueCommand(BackAsync, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            ShareCommand = new AsyncValueCommand(Shuffle, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            ShuffleCommand = new AsyncValueCommand(ShareAsync, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
        }
        #endregion

        #region Commands Methods

        private async Task Play()
        {
            if (IsPlaying)
            {
                await CrossMediaManager.Current.Pause();
                IsPlaying = false; ;
            }
            else
            {
                await CrossMediaManager.Current.Play();
                IsPlaying = true; ;
            }
        }

        private async ValueTask Shuffle()
        {
            if (NoShuffle)
            {
                NoShuffle = false;
                CrossMediaManager.Current.ShuffleMode = MediaManager.Queue.ShuffleMode.All;
                repeatMode = 1;
                await Repeat();

            }
            else
            {
                CrossMediaManager.Current.ShuffleMode = MediaManager.Queue.ShuffleMode.Off;
                repeatMode = 2;
                await Repeat();
            }
        }
        private async Task ChangeMusic(string value)
        {
            if (value == "P")
                await PreviousMusic();
            else if (value == "N")
                await NextMusic();
        }
        private async ValueTask BackAsync()
        {
            if (SelectedMusic == null)
                return;
            await _navigationService.NavigateToAsync<LandingViewModel>(new object[] { SelectedMusic });
        }
        private async ValueTask ShareAsync()
        {
            if (SelectedMusic == null)
                return;
            await Share.RequestAsync(new ShareFileRequest { File = new ShareFile(SelectedMusic.Url), Title = SelectedMusic.Title });
        }
        #endregion

        #region Private Methods
        private async Task Repeat()
        {
            switch (repeatMode)
            {
                case 0:
                    NoRepeat = false;
                    repeatOne = true;
                    repeatAll = false;
                    RepeatIcon = REPEATONEICON;
                    repeatMode = 1;
                    IsRepeatEnabled = true;
                    CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.One;
                    Shuffle(false);
                    break;
                case 1:
                    NoRepeat = false;
                    repeatOne = false;
                    repeatAll = true;
                    RepeatIcon = REPEATALLICON;
                    repeatMode = 2;
                    IsRepeatEnabled = true;
                    CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.Off;
                    break;
                case 2:
                    NoRepeat = true;
                    repeatOne = false;
                    repeatAll = false;
                    RepeatIcon = REPEATONEICON;
                    repeatMode = 0;
                    IsRepeatEnabled = false;
                    CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.Off;
                    Shuffle(false);
                    break;
                default:
                    break;
            }

            await Task.FromResult(true);
        }


        private void Shuffle(bool shuffle)
        {
            if (shuffle)
            {
                NoShuffle = false;
                CrossMediaManager.Current.ShuffleMode = MediaManager.Queue.ShuffleMode.All;

            }
            else
            {
                NoShuffle = true;
                CrossMediaManager.Current.ShuffleMode = MediaManager.Queue.ShuffleMode.Off;
            }
        }

        private async Task PlayMusic(Audio music)
        {
            try
            {
                var mediaInfo = CrossMediaManager.Current;
                await Task.Run(async () => await mediaInfo.Play(music?.Url));
                await mediaInfo.Play();
                Progress = 0;
                IsPlaying = true;
                Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                {
                    Duration = mediaInfo.Duration;
                    Maximum = Duration.TotalSeconds;
                    Position = mediaInfo.Position;

                    if (IsPlayTillActive && DateTime.Now.TimeOfDay >= SelectedTime)
                    {
                        IsPlayTillActive = false;
                        IsPlaying = false;

                        Task.Run(async () =>
                        {
                            mediaInfo.ShuffleMode = MediaManager.Queue.ShuffleMode.Off;
                            repeatMode = 2;
                            await Repeat();
                            await mediaInfo.Stop();
                        });
                    }
                    if (Maximum != default)
                    {
                        Progress = Position.TotalSeconds / Maximum;

                        if (Progress >= 1)
                        {
                            if (!repeatOne && !repeatAll)
                                IsPlaying = false;
                            if (repeatAll)
                                Task.Run(async () => await NextMusic());
                        }
                    }
                    return true;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async Task NextMusic()
        {
            try
            {
                var currentIndex = MusicList.IndexOf(SelectedMusic);
                if (currentIndex < MusicList.Count - 1)
                {
                    SelectedMusic = MusicList[currentIndex + 1];
                    await PlayMusic(SelectedMusic);
                }
                else
                {
                    if (repeatAll)
                    {
                        SelectedMusic = MusicList[0];
                        await PlayMusic(SelectedMusic);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async Task PreviousMusic()
        {
            try
            {
                var currentIndex = MusicList.IndexOf(SelectedMusic);

                if (currentIndex > 0)
                {
                    SelectedMusic = MusicList[currentIndex - 1];
                    await PlayMusic(SelectedMusic);
                }
                else
                {
                    await CrossMediaManager.Current.SeekToStart();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        #endregion

        #region Overrided Methods
        public override async Task InitializeAsync(object[] navigationData = null)
        {
            await CrossMediaManager.Current.Stop();
            IsPlaying = false;
            repeatMode = 0;
            NoRepeat = true;
            repeatOne = false;
            repeatAll = false;
            RepeatIcon = REPEATONEICON;
            NoShuffle = true;

            if (navigationData == null)
                return;

            SelectedMusic = navigationData[0] as Audio;
            MusicList = navigationData[1] as List<Audio>;

            await PlayMusic(SelectedMusic);
        }

        #endregion
    }
}
