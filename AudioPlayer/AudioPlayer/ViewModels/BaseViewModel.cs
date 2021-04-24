﻿using AudioPlayer.Models;
using AudioPlayer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AudioPlayer.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected readonly INavigationService _navigationService;

        public bool IsBusy { get; set; }
        public string Title { get; set; }

        public BaseViewModel()
        {
            _navigationService = DependencyService.Get<INavigationService>();
        }

        public virtual Task InitializeAsync(object[] navigationData = null) => Task.CompletedTask;

        public virtual Task UninitializeAsync(object[] navigationData = null) => Task.CompletedTask;

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


    }
}
