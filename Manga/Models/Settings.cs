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
    public sealed class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values["settings_" + name] = this.GetType().GetProperty(name).GetValue(this);
            System.Diagnostics.Debug.WriteLine("save:settings_" + name);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string _translate_code { get; set; } = "en-ru";
        public string translate_code
        {
            get { return _translate_code; }
            set { _translate_code = value; RaiseProperty("translate_code"); }
        }

        public string _translate_key { get; set; } = "";
        public string translate_key
        {
            get { return _translate_key; }
            set { _translate_key = value; RaiseProperty("translate_key"); }
        }

        public string _cache_size { get; set; } = "50";
        public string cache_size
        {
            get { return _cache_size; }
            set { _cache_size = value; RaiseProperty("cache_size"); }
        }

        public Settings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            foreach (var property in this.GetType().GetProperties())
            {
                //System.Diagnostics.Debug.WriteLine("field.Name:" + property.Name);

                if (property.Name[0] == '_')
                {
                    if (localSettings.Values.Keys.Contains("settings" + property.Name))
                    {
                        //System.Diagnostics.Debug.WriteLine("value:" + localSettings.Values["settings" + property.Name]);
                        property.SetValue(this, localSettings.Values["settings" + property.Name]);
                    }
                }
            }
        }
    }
}
