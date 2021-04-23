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
        private string outPath;
        private List<string> recordedFiles;
        private int _currentRecordNumber;
        private string audioSource;

        public string Timer { get; private set; }
        public Stopwatch StopWatch { get; private set; }

        public string AudioSource
        {
            get => audioSource;
            private set
            {
                audioSource = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string Name => FinishedRecording ? Path.GetFileNameWithoutExtension(AudioSource) : string.Empty;
        public bool FinishedRecording { get; private set; }
        public bool Playing { get; set; }
        public bool StartPlaying { get; set; }
        public string Icon { get; private set; }

        public ICommand PlayPauseCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand AudioPlayPauseCommand { get; private set; }
        public ICommand DeleteCommand => new Command(() => DeleteAsync());

        public ICommand BackCommand => new Command(async () => await BackAsync());
        public ICommand ShareCommand => new Command(
            async () => { if (!string.IsNullOrEmpty(AudioSource)) await Share.RequestAsync(new ShareFileRequest { File = new ShareFile(AudioSource), Title = Path.GetFileNameWithoutExtension(AudioSource) }); });

        public RecordViewModel()
        {
            StopWatch = new Stopwatch();
            PlayPauseCommand = new Command(PlayPause);
            recorder = new AudioRecorderService
            {
                StopRecordingAfterTimeout = false,
                StopRecordingOnSilence = false
            };
            StopCommand = new Command(async () => await Stop());
            recordedFiles = new List<string>();
        }

        public override async Task InitializeAsync(object[] navigationData = null)
        {
            await CrossMediaManager.Current.Stop();
            Icon = "\uf04b";
            if (navigationData == null)
            {

                _currentRecordNumber = Preferences.Get("Count", 0);
                return;
            }

            _currentRecordNumber = (int)navigationData[0];

        }
        private void DeleteAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(AudioSource) && File.Exists(AudioSource))
                {
                    File.Delete(AudioSource);
                    CrossMediaManager.Current.Stop();
                    FinishedRecording = false;
                    recordedFiles.Clear();
                    StopWatch.Reset();
                }
            }
            catch (Exception deleteEx)
            { }
        }

        public async Task BackAsync()
        {
            if (!FinishedRecording)
                await Stop();
            await _navigationService.NavigateToAsync<LandingViewModel>();

        }

        private async Task Stop()
        {
            Playing = false;
            StopWatch.Reset();
            await Task.Run(async () =>
            {
                try
                {
                    if (recorder != null)
                    {
                        if (recorder.IsRecording)
                            await recorder.StopRecording();

                        if (!string.IsNullOrEmpty(recorder.GetAudioFilePath()))
                            recordedFiles.Add(recorder.GetAudioFilePath());
                        if (recordedFiles.Count > 0)
                        {
                            if (recordedFiles.Count > 1)
                            {
                                if (!string.IsNullOrEmpty(recorder.GetAudioFilePath()) && recordedFiles.Last() != recorder.GetAudioFilePath())
                                    recordedFiles.Add(recorder.GetAudioFilePath());

                                outPath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.wav");
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
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(recorder.GetAudioFilePath()))
                                {
                                    outPath = recorder.GetAudioFilePath();
                                }
                            }

                            if (!string.IsNullOrEmpty(outPath))
                            {
                                try
                                {
                                    var newPath = Path.Combine(FileSystem.AppDataDirectory, $"Record-{_currentRecordNumber}.wav");
                                    File.Move(outPath, newPath);
                                    _currentRecordNumber++;
                                    Preferences.Set("Count", _currentRecordNumber);
                                    Device.BeginInvokeOnMainThread(() =>
                                        {
                                            FinishedRecording = true;
                                            AudioSource = newPath;
                                        });
                                }
                                catch (Exception copyEx)
                                {
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    //Device.BeginInvokeOnMainThread(() =>
                    //{
                    //    StopWatch.Reset();
                    //    Playing = false;

                    //});
                    recordedFiles.Clear();
                }
            });
        }


        private async void PlayPause(object obj)
        {

            await RecordAudio();
        }


        async Task RecordAudio()
        {

            try
            {
                await Task.Run(async () =>
                {
                    FinishedRecording = false;
                    if (!recorder.IsRecording) //Record button clicked
                    {

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Playing = true;
                            StopWatch.Start();

                            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                            {
                                Timer = StopWatch.Elapsed.ToString("hh\\:mm\\:ss");
                                if (!recorder.IsRecording)
                                {
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            });
                        });

                        recorder.FilePath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.wav");

                        var audioRecordTask = await recorder.StartRecording();
                        await audioRecordTask;
                    }
                    else //Stop button clicked
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Playing = false;
                            StopWatch.Stop();
                        });
                        await recorder.StopRecording();
                        if (!string.IsNullOrEmpty(recorder.GetAudioFilePath()))
                            recordedFiles.Add(recorder.GetAudioFilePath());
                    }
                });
            }
            catch (Exception ex)
            { }
        }
    }
}
