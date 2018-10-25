using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Manga.Helpers
{
    public class UnVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                if (value is Visibility)
                {
                    Visibility visibility = (Visibility) value;
                    if (visibility == Visibility.Collapsed)
                    {
                        return Visibility.Visible;
                    } else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
