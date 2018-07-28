using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Manga.Models
{
    // in coding.....
    class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            Type type = typeof(Settings);
            FieldInfo[] fields = type.GetFields();
            FieldInfo field = Array.Find(fields, (f) => { return f.Name == name; });

            localSettings.Values["settings_" + name] = field.GetValue(this);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Settings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            Type type = typeof(Settings);
            FieldInfo[] fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.Name[0] == '_')
                {
                    if (localSettings.Values.Keys.Contains("settings" + field.Name))
                    {
                        field.SetValue(this, localSettings.Values["settings" + field.Name]);
                    }
                }
            }
        }
    }
}
