using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace AudioPlayer.Helpers
{
    public class VideoFileHelpers
    {
        // This method copies the video from the app package to the app data
        // directory for your app. To copy the video to the temp directory
        // for your app, comment out the first line of code, and uncomment
        // the second line of code.
        public static async Task<string> CopyVideoIfNotExists(string filename)
        {
            //string folder = FileSystem.AppDataDirectory;
            string folder = Path.GetTempPath();
            string videoFile = Path.Combine(folder, $"{Guid.NewGuid()}.wav");
            //string videoFile = Path.Combine(folder, filename);

            if (!File.Exists(videoFile))
            {
                //using (Stream inputStream = await FileSystem.OpenAppPackageFileAsync(filename))
                using (Stream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read))
                {
                    using (FileStream outputStream = File.Create(videoFile))
                    {
                        await inputStream.CopyToAsync(outputStream);
                    }
                }
                return videoFile;
            }

            return null;
        }
    }
}
