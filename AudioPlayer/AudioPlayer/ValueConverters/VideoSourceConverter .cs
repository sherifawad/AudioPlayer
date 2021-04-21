using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AudioPlayer.ValueConverters
{
    /// <summary>
    /// A converter that takes in a boolean and inverts it
    /// </summary>
    public class VideoSourceConverter : BaseValueConverter<VideoSourceConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (string.IsNullOrWhiteSpace(value.ToString()))
                return null;

            return new Uri($"ms-appdata:///local/{value}");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
