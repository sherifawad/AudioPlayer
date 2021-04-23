using AudioPlayer.Extensions;
using AudioPlayer.Models;
using MediaManager;
using Plugin.AudioRecorder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
        #endregion

        #region public Properties
        public double Progress { get; set; }


        public Audio SelectedMusic { get; set; }
        public bool NoShuffle { get; set; }
        public bool NoRepeat { get; set; }
        public string RepeatIcon { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Position { get; set; }
        public bool IsRepeatEnabled { get; private set; }
        public bool IsPlayTillActive
        {
            get => isPlayTillActive;
            set
            {
                isPlayTillActive = value;
                if (value)
                    SelectedTime = DateTime.Now.TimeOfDay + TimeSpan.FromMinutes(2);
            }
        }
        public TimeSpan SelectedTime { get; set; }

        public double Maximum
        {
            get { return maximum; }
            set
            {
                if (value > 0)
                {
                    maximum = value;
                }
            }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                isPlaying = value;
                OnPropertyChanged(nameof(PlayIcon));
                MessagingCenter.Send(MessengerKeys.App, MessengerKeys.Play, IsPlaying);
            }
        }

        public string PlayIcon { get => isPlaying ? "pause.png" : "play.png"; }

        #endregion


        #region Commands Properties

        public ICommand RepeatCommand => new Command(() => Repeat());
        public ICommand ShuffleCommand => new Command(() => Shuffle());

        public ICommand PlayCommand => new Command(Play);
        public ICommand ChangeCommand => new Command(async (parameter) => await ChangeMusic(parameter));
        public ICommand BackCommand => new Command(async () => await _navigationService.NavigateToAsync<LandingViewModel>(new object[] { SelectedMusic }));
        public ICommand ShareCommand => new Command(() => Share.RequestAsync(new ShareFileRequest { File = new ShareFile(SelectedMusic.Url), Title = SelectedMusic.Title }));
        #endregion


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
        private void Repeat()
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
        }

        private void Shuffle()
        {
            if (NoShuffle)
            {
                NoShuffle = false;
                CrossMediaManager.Current.ShuffleMode = MediaManager.Queue.ShuffleMode.All;
                repeatMode = 1;
                Repeat();

            }
            else
            {
                CrossMediaManager.Current.ShuffleMode = MediaManager.Queue.ShuffleMode.Off;
                repeatMode = 2;
                Repeat();
            }
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

        private async void Play()
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

        private async Task ChangeMusic(object obj)
        {
            try
            {
                if ((string)obj == "P")
                    await PreviousMusic();
                else if ((string)obj == "N")
                    await NextMusic();
            }
            catch (Exception ex)
            { }
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
                        mediaInfo.ShuffleMode = MediaManager.Queue.ShuffleMode.Off;
                        repeatMode = 2;
                        Repeat();
                        Task.Run(async () => await mediaInfo.Stop());
                        //mediaInfo.Stop().SafeFireAndForget();
                    }
                    if (Maximum != default)
                    {
                        Progress = Position.TotalSeconds / Maximum;
                        //var ticked = Position.TotalSeconds >= 0;

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
            { }
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
            { }
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
            { }
        }
    }
}
