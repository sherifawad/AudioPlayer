using AudioPlayer.Models;
using MediaManager;
using Plugin.AudioRecorder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AudioPlayer.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        #region private Properties
        private readonly List<Audio> MusicList;
        private const string REPEATONEICON = "\uf021";
        private const string REPEATALLICON = "\uf079";
        double maximum = 100f;
        private bool isPlaying;
        private bool repeatOne;
        private bool repeatAll;
        private int repeatMode;

        #endregion


        #region Properties
        public Audio SelectedMusic { get; set; }
        public bool NoShuffle { get; set; }
        public bool NoRepeat { get; set; }
        public string RepeatIcon { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Position { get; set; }

        public double Maximum
        {
            get { return maximum; }
            set
            {
                if (value > 0)
                {
                    maximum = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                isPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlayIcon));
            }
        }

        public string PlayIcon { get => isPlaying ? "pause.png" : "play.png"; }

        #endregion


        #region Commands Properties

        public ICommand RepeatCommand => new Command(() => Repeat());
        public ICommand ShuffleCommand => new Command(() => Shuffle());

        public ICommand PlayCommand => new Command(Play);
        public ICommand ChangeCommand => new Command(ChangeMusic);
        public ICommand BackCommand => new Command(() => Application.Current.MainPage.Navigation.PopAsync());
        public ICommand ShareCommand => new Command(() => Share.RequestAsync(SelectedMusic.Url, SelectedMusic.Title));
        #endregion

        public PlayerViewModel(Audio selectedMusic, List<Audio> musicList)
        {
            SelectedMusic = selectedMusic;
            MusicList = musicList;
            PlayMusic(SelectedMusic);
            isPlaying = true;
            repeatMode = 0;
            NoRepeat = true;
            repeatOne = false;
            repeatAll = false;
            RepeatIcon = REPEATONEICON;
            NoShuffle = true;
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
                    CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.One;
                    Shuffle(false);
                    break;
                case 1:
                    NoRepeat = false;
                    repeatOne = false;
                    repeatAll = true;
                    RepeatIcon = REPEATALLICON;
                    repeatMode = 2;
                    CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.Off;
                    break;
                case 2:
                    NoRepeat = true;
                    repeatOne = false;
                    repeatAll = false;
                    RepeatIcon = REPEATONEICON;
                    repeatMode = 0;
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

        private void ChangeMusic(object obj)
        {
            try
            {
                if ((string)obj == "P")
                    PreviousMusic();
                else if ((string)obj == "N")
                    NextMusic();
            }
            catch (Exception ex)
            { }
        }

        private async void PlayMusic(Audio music)
        {
            try
            {
                var mediaInfo = CrossMediaManager.Current;
                await mediaInfo.Play(music?.Url);
                IsPlaying = true;

                mediaInfo.MediaItemFinished += (sender, args) =>
                {
                    if (!repeatOne)
                        IsPlaying = false;
                    if (repeatAll)
                        NextMusic();
                };

                Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                {
                    Duration = mediaInfo.Duration;
                    Maximum = Duration.TotalSeconds;
                    Position = mediaInfo.Position;
                    //if (repeatAll && mediaInfo.RepeatMode != MediaManager.Playback.RepeatMode.All)
                    //    mediaInfo.RepeatMode = MediaManager.Playback.RepeatMode.All;
                    //else if (repeatOne && mediaInfo.RepeatMode != MediaManager.Playback.RepeatMode.One)
                    //    mediaInfo.RepeatMode = MediaManager.Playback.RepeatMode.All;
                    //else if (NoRepeat && mediaInfo.RepeatMode != MediaManager.Playback.RepeatMode.Off)
                    //    mediaInfo.RepeatMode = MediaManager.Playback.RepeatMode.Off;
                    //if (NoShuffle && mediaInfo.ShuffleMode != MediaManager.Queue.ShuffleMode.Off)
                    //    mediaInfo.ShuffleMode = MediaManager.Queue.ShuffleMode.Off;
                    //else if (!NoShuffle && mediaInfo.ShuffleMode != MediaManager.Queue.ShuffleMode.All)
                    //    mediaInfo.ShuffleMode = MediaManager.Queue.ShuffleMode.All;
                    return true;
                });
            }
            catch (Exception ex)
            { }
        }

        private void NextMusic()
        {
            try
            {
                var currentIndex = MusicList.IndexOf(SelectedMusic);
                if (currentIndex < MusicList.Count - 1)
                {
                    SelectedMusic = MusicList[currentIndex + 1];
                    PlayMusic(SelectedMusic);
                }
                else
                {
                    if (repeatAll)
                    {
                        SelectedMusic = MusicList[0];
                        PlayMusic(SelectedMusic);
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private void PreviousMusic()
        {
            try
            {
                var currentIndex = MusicList.IndexOf(SelectedMusic);

                if (currentIndex > 0)
                {
                    SelectedMusic = MusicList[currentIndex - 1];
                    PlayMusic(SelectedMusic);
                }
            }
            catch (Exception ex)
            { }
        }
    }
}
