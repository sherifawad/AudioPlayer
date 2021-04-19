using AudioPlayer.Models;
using AudioPlayer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace AudioPlayer.ViewModels
{
    public class LandingViewModel : BaseViewModel
    {
        public LandingViewModel()
        {
            MusicList = new List<Audio>();
            GetMusics();
            RecentMusic = MusicList.Where(x => x.IsRecent == true).FirstOrDefault();
        }

        public List<Audio> MusicList { get; private set; }

        public Audio RecentMusic { get; set; }

        public Audio SelectedMusic { get; set; }

        public ICommand SelectionCommand => new Command(async () => await PlayMusic());
        public ICommand NewRecordCommand => new Command(async () => await RecordAsync());

        private async Task PlayMusic()
        {
            if (SelectedMusic != null && !IsBusy)
            {
                var viewModel = new PlayerViewModel(SelectedMusic, MusicList);
                var playerPage = new PlayerPage { BindingContext = viewModel };

                var navigation = Application.Current.MainPage as NavigationPage;
                await navigation.PushAsync(playerPage, true);

                //await Shell.Current.GoToAsync(nameof(playerPage), true);
            }
        }
        private async Task RecordAsync()
        {
            var navigation = Application.Current.MainPage as NavigationPage;
            var recordPage = new RecordPage();

            await navigation.PushAsync(recordPage, true);
        }

        private List<Audio> GetMusics()
        {
            IsBusy = true;
            try
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string wavPattern = ".wav";

                var FileList = Directory.GetFiles(dir);

                if (FileList != null)
                {
                    for (int i = 0; i < FileList.Length; i++)
                    {
                        if (FileList[i].EndsWith(wavPattern))
                        {
                            MusicList.Add(new Audio { Title = Path.GetFileNameWithoutExtension(FileList[i]), Url = FileList[i] });
                        }
                    }
                }
            }
            catch(Exception ex) 
            { }
            finally
            {
                IsBusy = false;
            }
            return MusicList;
            //return new ObservableCollection<Audio>
            //{
            //    new Audio { Title = "Beach Walk", Artist = "Unicorn Heads", Url = "https://devcrux.com/wp-content/uploads/Beach_Walk.mp3", CoverImage = "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcRU6FVly4jMTD3AKB_sHxqPofJVQwqqUj5peEvgA1H4XegM3uJ7&usqp=CAU", IsRecent = true},
            //    new Audio { Title = "I'll Follow You", Artist = "Density & Time", Url = "https://devcrux.com/wp-content/uploads/I_ll_Follow_You.mp3", CoverImage = "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcRm-su97lHFGZrbR6BkgL32qbzZBj2f3gKGrFR0Pn66ih01SyGj&usqp=CAU"},
            //    new Audio { Title = "Ancient", Artist = "Density & Time", Url = "https://devcrux.com/wp-content/uploads/Ancient.mp3"},
            //    new Audio { Title = "News Room News", Artist = "Spence", Url = "https://devcrux.com/wp-content/uploads/Cats_Searching_for_the_Truth.mp3"},
            //    new Audio { Title = "Bro Time", Artist = "Nat Keefe & BeatMowe", Url = "https://devcrux.com/wp-content/uploads/Bro_Time.mp3"},
            //    new Audio { Title = "Cats Searching for the Truth", Artist = "Nat Keefe & Hot Buttered Rum", Url = "https://devcrux.com/wp-content/uploads/Cats_Searching_for_the_Truth.mp3"}
            //};
        }
    }
}
