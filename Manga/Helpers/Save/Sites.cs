using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace Manga.Helpers.Save
{
    class Sites
    {
        public static void Save(ObservableCollection<Models.Site> Sites)
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

            foreach (Models.Site site in Sites)
            {
                site.Save();
            }
        }

        public static void Load(ref ObservableCollection<Models.Site> Sites)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            foreach (string key in localSettings.Values.Keys)
            {
                if (!key.Contains("site_"))
                {
                    continue;
                }

                Sites.Add(
                    new Models.Site(
                        JsonValue.Parse(localSettings.Values[key].ToString()).GetObject()
                        )
                    );
            }
        }

        public static List<string> AsStrings()
        {
            List<string> list = new List<string>();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            foreach (string key in localSettings.Values.Keys)
            {
                if (!key.Contains("site_"))
                {
                    continue;
                }

                list.Add(localSettings.Values[key].ToString());
            }

            return list;
        }
    }
}
