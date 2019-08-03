using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace Manga.Models
{
    class Config
    {
        public const string CONFIG_LINK = "https://raw.githubusercontent.com/ta-tikoma/Manga4you/master/Manga/Assets/sites.json";

        public static async Task CheckAsync()
        {
            if (!NeedUpdate())
            {
                return;
            }

            await DownloadAsync();
        }

        public static bool NeedUpdate()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("date_last_update"))
            {
                string date_last_update = localSettings.Values["date_last_update"].ToString();
                DateTime convertedDate;
                try
                {
                    convertedDate = Convert.ToDateTime(date_last_update);
                    DateTime currentDate = DateTime.Now;
                    TimeSpan span = currentDate - convertedDate;
                    if (span.Days < 7)
                    {
                        return false;
                    }
                }
                catch (FormatException)
                {
                }
            }
            return true;
        }

        public static string AsString()
        {
            List<string> sites = new List<string>();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            foreach (string key in localSettings.Values.Keys)
            {
                if (!key.Contains("site_"))
                {
                    continue;
                }
                sites.Add(localSettings.Values[key].ToString());
            }

            return "[\n" + String.Join(",\n", sites.ToArray()) + "\n]";
        }

        public static void Load(string configContent)
        {
            ObservableCollection<Site> Sites = new ObservableCollection<Site>();
            JsonArray jsonValues = JsonValue.Parse(configContent).GetArray();
            foreach (JsonObject jsonObject in jsonValues)
            {
                Sites.Add(
                    new Site(jsonObject)
                );
            }

            Site.SaveList(Sites);

            DateTime currentDate = DateTime.Now;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["date_last_update"] = currentDate.ToString();
        }

        public static async Task DownloadAsync()
        {
            System.Diagnostics.Debug.WriteLine("SetDefaultSites");

            string res = await Helpers.Request.rh.Get(CONFIG_LINK);
            if (res == null)
            {
                System.Diagnostics.Debug.WriteLine("(res == null)");
                return;
            }

            Load(res);
        }
    }
}
