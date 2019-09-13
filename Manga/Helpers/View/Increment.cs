using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Manga.Helpers.View
{
    public class Increment : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int int_value = 0;
            if (int.TryParse(value.ToString(), out int_value))
            {
                return (int_value + 1).ToString();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}