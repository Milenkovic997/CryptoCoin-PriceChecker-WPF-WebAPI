using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Crypto_NFT_PriceChecker_WPF_WebAPI.CryptoDescription5m
{
    public class TextToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString().StartsWith("-")) return new SolidColorBrush(Colors.Red);
            else if(value.ToString().Equals("0 %")) return new SolidColorBrush(Colors.White);
            else return new SolidColorBrush(Colors.Green);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
