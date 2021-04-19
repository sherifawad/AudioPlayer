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
        AudioRecorderService recorder;

        #endregion
        public PlayerViewModel(Audio selectedMusic, List<Audio> musicList)
        {
            SelectedMusic = selectedMusic;
            MusicList = musicList;
            PlayMusic(SelectedMusic);
            isPlaying = true;


        }


        #region Properties
        public Audio SelectedMusic { get; set; }

        public TimeSpan Duration { get; set; }
        public TimeSpan Position { get; set; }

        double maximum = 100f;
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


        private bool isPlaying;
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

        public ICommand PlayCommand => new Command(Play);
        public ICommand ChangeCommand => new Command(ChangeMusic);
        public ICommand BackCommand => new Command(() => Application.Current.MainPage.Navigation.PopAsync());
        public ICommand ShareCommand => new Command(() => Share.RequestAsync(SelectedMusic.Url, SelectedMusic.Title));


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
                    IsPlaying = false;
                    NextMusic();
                };

                Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                {
                    Duration = mediaInfo.Duration;
                    Maximum = Duration.TotalSeconds;
                    Position = mediaInfo.Position;
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
