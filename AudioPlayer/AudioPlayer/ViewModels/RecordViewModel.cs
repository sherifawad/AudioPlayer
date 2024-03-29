﻿using AudioPlayer.Helpers;
using AudioPlayer.Services;
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
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AudioPlayer.ViewModels
{
    public class RecordViewModel : BaseViewModel
    {
        #region Private Properties

        private readonly AudioRecorderService recorder;
        private string outPath;
        private List<string> recordedFiles;
        private int _currentRecordNumber;
        private string audioSource;
        private bool Calculate;
        private string timer;
        private Stopwatch stopWatch;
        private bool finishedRecording;
        private bool playing;
        private bool startPlaying;
        private string icon;
        private readonly INavigationService _navigationService;
        #endregion

        #region Public Properties

        public string Timer
        {
            get => timer;
            private set => SetProperty(ref timer, value);
        }
        public Stopwatch
            StopWatch
        {
            get => stopWatch;
            private set => SetProperty(ref stopWatch, value);
        }

        public string AudioSource
        {
            get => audioSource;
            private set
            {
                SetProperty(ref audioSource, value);
                OnPropertyChanged(nameof(Name));
            }
        }
        public string Name => FinishedRecording ? Path.GetFileNameWithoutExtension(AudioSource) : string.Empty;
        public bool FinishedRecording
        {
            get => finishedRecording;
            private set => SetProperty(ref finishedRecording, value);
        }
        public bool Playing
        {
            get => playing;
            set => SetProperty(ref playing, value);
        }
        public bool StartPlaying
        {
            get => startPlaying;
            set => SetProperty(ref startPlaying, value);
        }
        public string Icon
        {
            get => icon;
            private set => SetProperty(ref icon, value);
        }
        #endregion

        #region Public Commands
        public IAsyncCommand PlayPauseCommand { get; }
        public IAsyncCommand StopCommand { get; }
        public IAsyncCommand DeleteCommand { get; }
        public IAsyncCommand BackCommand { get; }
        public IAsyncValueCommand ShareCommand { get; }

        #endregion

        #region Default Constructo
        public RecordViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            recordedFiles = new List<string>();
            StopWatch = new Stopwatch();
            recorder = new AudioRecorderService
            {
                StopRecordingAfterTimeout = false,
                SilenceThreshold = 0,
                StopRecordingOnSilence = true
                
            };
            PlayPauseCommand = new AsyncCommand(RecordAudio, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            StopCommand = new AsyncCommand(Stop, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            DeleteCommand = new AsyncCommand(DeleteAsync, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            BackCommand = new AsyncCommand(BackAsync, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            ShareCommand = new AsyncValueCommand(ShareAsync, onException: ex => Debug.WriteLine(ex), allowsMultipleExecutions: false);
            Icon = "\uf04b";
            recorder.AudioInputReceived += Recorder_AudioInputReceived;
            _currentRecordNumber = Preferences.Get("Count", 0);
        }
        #endregion

        #region Commands Methods
        private async ValueTask ShareAsync()
        {
            if (string.IsNullOrEmpty(AudioSource))
                return;

            await Share.RequestAsync(new ShareFileRequest { File = new ShareFile(AudioSource), Title = Path.GetFileNameWithoutExtension(AudioSource) });
        }
        private async Task DeleteAsync()
        {
            if (!string.IsNullOrEmpty(AudioSource) && File.Exists(AudioSource))
            {
                File.Delete(AudioSource);
                await CrossMediaManager.Current.Stop();
                FinishedRecording = false;
                recordedFiles.Clear();
                StopWatch.Reset();
            }
        }
        public async Task BackAsync()
        {
            try
            {
                if (!FinishedRecording)
                {
                    if (recorder.IsRecording)
                    {
                        Playing = false;
                        Calculate = true;
                        await recorder.StopRecording();
                    }

                }
                recorder.AudioInputReceived -= Recorder_AudioInputReceived;
                await CrossMediaManager.Current.Stop();
                Timer = string.Empty;
                //AudioSource = string.Empty;
                outPath = string.Empty;
                FinishedRecording = false;
                StartPlaying = false;
                Playing = false;
                recordedFiles.Clear();
                stopWatch.Reset();
                await _navigationService.NavigateToAsync<LandingViewModel>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }
        private async Task Stop()
        {
            Playing = false;
            Calculate = true;
            StopWatch.Stop();
            await recorder.StopRecording();
        }
        async Task RecordAudio()
        {
            FinishedRecording = false;
            Calculate = false;
            if (!recorder.IsRecording) //Record button clicked
            {

                recorder.FilePath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.wav");

                var audioRecordTask = await recorder.StartRecording();
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
            }
            else
            {
                Playing = false;
                StopWatch.Stop();
                await recorder.StopRecording();
            }
        }
        #endregion

        #region Overrided Methods
        public override async Task InitializeAsync(object[] navigationData = null)
        {
            try
            {
                await CrossMediaManager.Current.Stop();
                Icon = "\uf04b";
                recorder.AudioInputReceived += Recorder_AudioInputReceived;
                _currentRecordNumber = Preferences.Get("Count", 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        //public override async Task UninitializeAsync()
        //{
        //    try
        //    {
        //        recorder.AudioInputReceived -= Recorder_AudioInputReceived;
        //        await CrossMediaManager.Current.Stop();
        //        Timer = string.Empty;
        //        //AudioSource = string.Empty;
        //        outPath = string.Empty;
        //        FinishedRecording = false;
        //        StartPlaying = false;
        //        Playing = false;
        //        recordedFiles.Clear();
        //        stopWatch.Reset();
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //    }
        //}

        #endregion

        #region Private Methods

        private async void Recorder_AudioInputReceived(object sender, string audioFile)
        {
            if (!string.IsNullOrEmpty(audioFile))
                recordedFiles.Add(audioFile);
            if (Calculate && recordedFiles.Count > 0)
                OnCalculate();
            await Task.FromResult(true);
        }
        private void OnCalculate()
        {
            try
            {
                if (recordedFiles.Count <= 0)
                    return;

                StopWatch.Reset();
                recordedFiles = recordedFiles.Distinct().ToList();
                if (recordedFiles.Count > 1)
                {
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
                            {
                                Debug.WriteLine(deleteEx.Message);
                            }
                        }
                    }
                }
                else if (recordedFiles.Count == 1)
                {
                    outPath = recordedFiles[0];
                }

                if (!string.IsNullOrEmpty(outPath))
                {
                    try
                    {
                        _currentRecordNumber++;
                        var newPath = Path.Combine(FileSystem.AppDataDirectory, $"Record-{_currentRecordNumber}.wav");
                        File.Move(outPath, newPath);
                        Preferences.Set("Count", _currentRecordNumber);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            FinishedRecording = true;
                            AudioSource = newPath;
                        });
                    }
                    catch (Exception copyEx)
                    {
                        Debug.WriteLine(copyEx);
                    }
                }

                recordedFiles.Clear();
            }
            catch (Exception exx)
            {
                Debug.WriteLine(exx);
            }
        }
        #endregion
    }
}
