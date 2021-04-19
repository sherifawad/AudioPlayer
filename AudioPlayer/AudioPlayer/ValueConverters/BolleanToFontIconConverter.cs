using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace AudioPlayer.ValueConverters
{
    public class BolleanToFontIconConverter : BaseValueConverter<BolleanToFontIconConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return new FontImageSource { FontFamily = "SolidAwesome", Glyph = "\uf04c" };

            return new FontImageSource { FontFamily = "SolidAwesome", Glyph = "\uf04b", Color = Color.Black };
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
