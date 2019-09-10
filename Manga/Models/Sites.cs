using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace Manga.Models
{
    class Sites
    {
        public static void Save(ObservableCollection<Site> Sites)
        {
            // как то странно
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            foreach (string key in localSettings.Values.Keys)
            {
                if (!key.Contains("site_"))
                {
                    continue;
                }
                localSettings.Values.Remove(key);
            }

            foreach (Site site in Sites)
            {
                site.Save();
            }
        }

        public static void Load(ref ObservableCollection<Site> Sites)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            foreach (string key in localSettings.Values.Keys)
            {
                if (!key.Contains("site_"))
                {
                    continue;
                }

                Sites.Add(
                    new Site(
                        JsonValue.Parse(localSettings.Values[key].ToString()).GetObject()
                        )
                    );
            }
        }
    }
}
