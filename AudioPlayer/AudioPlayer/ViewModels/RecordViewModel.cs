using AudioPlayer.Helpers;
using MediaManager;
using Plugin.AudioRecorder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AudioPlayer.ViewModels
{
    public class RecordViewModel : BaseViewModel
    {
        private AudioRecorderService recorder;
        private Stopwatch stopwatch;
        private string outPath;
        private IMediaManager mediaInfo;
        private bool repeat;
        private string maxCount;
        private List<string> recordedFiles;
        private string PauseIcon = "\uf04c";
        private string PalyIcon = "\uf04b";
        private int _currentRecordNumber;
        double maximum = 100f;

        public string AudioSource { get; private set; }
        public TimeSpan SelectedTime { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Position { get; set; }
        public string MaxCount
        {
            get => maxCount;
            set
            {
                maxCount = Regex.Replace(value, @"[^\d]", "");
                CrossMediaManager.Current.Stop();
                StartPlaying = false;
            }
        }

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
        public string Timer { get; private set; }
        public bool IsPlayTillActive { get; set; }
        public bool FinishedRecording { get; private set; }

        public string Icon { get; set; }
        public bool Repeat
        {
            get => repeat;
            set
            {
                repeat = value;
                CrossMediaManager.Current.Stop();
                StartPlaying = false;
            }
        }
        public bool IsPlaying { get; set; }
        public bool StartPlaying { get; set; }
        public string PlayImage { get => IsPlaying ? "pause.png" : "play.png"; }

        public ICommand PlayPauseCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand AudioPlayPauseCommand { get; private set; }
        public ICommand BackCommand => new Command(async () => await BackAsync());
        public ICommand ShareCommand => new Command(
            async () => { if (!string.IsNullOrEmpty(outPath)) await Share.RequestAsync(AudioSource, Path.GetFileNameWithoutExtension(AudioSource)); });

        public RecordViewModel()
        {
            stopwatch = new Stopwatch();
            PlayPauseCommand = new Command(PlayPause);
            PlayCommand = new Command(Play);
            recorder = new AudioRecorderService
            {
                StopRecordingOnSilence = true
            };
            AudioPlayPauseCommand = new Command(() => playAudio());
            StopCommand = new Command(async () => await Stop());
            recordedFiles = new List<string>();
        }

        public override async Task InitializeAsync(object[] navigationData = null)
        {
            Icon = PalyIcon;
            await CrossMediaManager.Current.Stop();

            if (navigationData == null)
                return;

            _currentRecordNumber = (int)navigationData[0];

        }
        public async Task BackAsync()
        {
            if (!FinishedRecording)
                await Stop();
            await _navigationService.NavigateToAsync<LandingViewModel>();

        }

        private async Task Stop()
        {

            IsPlaying = false;
            if (recorder != null)
            {
                await recorder.StopRecording();
                if (!string.IsNullOrEmpty(recorder.GetAudioFilePath()))
                {
                    if (recordedFiles.Count > 0)
                    {
                        if (recordedFiles.Last() != recorder.GetAudioFilePath())
                            recordedFiles.Add(recorder.GetAudioFilePath());
                    }
                    else
                    {
                        recordedFiles.Add(recorder.GetAudioFilePath());
                    }
                }
                if (stopwatch != null) stopwatch.Stop();
            }
            try
            {
                if (recordedFiles.Count > 0)
                {
                    if (recordedFiles.Count > 1)
                    {
                        outPath = Path.Combine(FileSystem.AppDataDirectory, $"{Guid.NewGuid()}.wav");
                        var result = WaveFilesHelpers.Merge(recordedFiles, outPath);
                        if (result != null)
                        {
                            foreach (var file in recordedFiles)
                            {
                                try
                                {
                                    if (File.Exists(file))
                                        File.Delete(file);
                                }
                                catch (Exception deleteEx)
                                { }
                            }
                            recordedFiles.Clear();
                        }
                    }
                    else
                    {
                        outPath = recordedFiles[0];
                    }

                    if (!string.IsNullOrEmpty(outPath))
                    {
                        try
                        {
                            var newPath = Path.Combine(FileSystem.AppDataDirectory, $"Record-{_currentRecordNumber}.wav");
                            File.Move(outPath, newPath);
                            AudioSource = newPath;
                            FinishedRecording = true;
                        }
                        catch (Exception copyEx)
                        {
                            stopwatch.Reset();
                        }
                    }

                }
                else
                {
                    stopwatch.Reset();
                    IsPlaying = false;
                }

            }
            catch (Exception ex)
            {
                stopwatch.Reset();
                IsPlaying = false;
            }
        }

        private async void playAudio()
        {
            if (IsPlaying)
            {
                await CrossMediaManager.Current.Pause();
                Icon = PauseIcon;
                IsPlaying = false;
            }
            else
            {
                await CrossMediaManager.Current.Play();
                Icon = PalyIcon;
                IsPlaying = true;
            }
        }
        private async void Play()
        {
            try
            {
                if (string.IsNullOrEmpty(outPath))
                    return;
                int count = 0;
                if (!string.IsNullOrEmpty(MaxCount))
                    count = int.Parse(MaxCount);

                mediaInfo = CrossMediaManager.Current;
                if (Repeat && (IsPlayTillActive || count > 0))
                    mediaInfo.ToggleRepeat();
                StartPlaying = true;
                await mediaInfo.Play(outPath);
                DateTime? total = null;
                bool getDuration = false;

                Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                {
                    Duration = mediaInfo.Duration;
                    Maximum = Duration.TotalSeconds;
                    Position = mediaInfo.Position;
                    if (Repeat && count > 0)
                    {
                        if (!getDuration && Duration.Ticks > 0)
                        {
                            total = DateTime.Now + TimeSpan.FromTicks(Duration.Ticks * count);
                            getDuration = true;
                        }
                    }
                    return true;
                });

                mediaInfo.PositionChanged += (sender, args) =>
                {
                    if (Repeat && IsPlayTillActive && DateTime.Now.TimeOfDay >= SelectedTime)
                    {
                        Repeat = false;
                        IsPlayTillActive = false;
                        mediaInfo.ToggleRepeat();
                        mediaInfo.Stop();
                        StartPlaying = false;
                    }
                    if (total != null && DateTime.Now >= total)
                    {
                        Repeat = false;
                        IsPlayTillActive = false;
                        mediaInfo.ToggleRepeat();
                        mediaInfo.Stop();
                        StartPlaying = false;
                    }
                };

                mediaInfo.MediaItemFinished += (sender, args) =>
                {
                    IsPlaying = false;
                    //_repeated++;
                    //if (Repeat && count > 0 && _repeated >= count)
                    //{
                    //    Repeat = false;
                    //    IsPlayTillActive = false;
                    //    mediaInfo.ToggleRepeat();
                    //    mediaInfo.Stop();
                    //    StartPlaying = false;
                    //}
                };
            }
            catch (Exception ex)
            { }
        }

        private async void PlayPause(object obj)
        {

            await RecordAudio();
        }


        async Task RecordAudio()
        {
            try
            {
                if (recorder != null && !recorder.IsRecording) //Record button clicked
                {
                    //recorder.FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{Guid.NewGuid()}.wav");
                    recorder.FilePath = Path.Combine(FileSystem.AppDataDirectory, $"{Guid.NewGuid()}.wav");

                    //start recording audio
                    var audioRecordTask = await recorder.StartRecording();
                    stopwatch.Start();
                    Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
                    {
                        Timer = stopwatch.Elapsed.ToString("hh\\:mm\\:ss");
                        if (!stopwatch.IsRunning)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    });
                    Icon = PauseIcon;
                    await audioRecordTask;

                }
                else //Stop button clicked
                {

                    if (recorder != null)
                    {
                        await recorder.StopRecording();
                        if (!string.IsNullOrEmpty(recorder.GetAudioFilePath()))
                            recordedFiles.Add(recorder.GetAudioFilePath());
                        if (stopwatch != null) stopwatch.Stop();
                    }
                    Icon = PalyIcon;
                }
            }
            catch (Exception ex)
            { }
        }
    }
}
