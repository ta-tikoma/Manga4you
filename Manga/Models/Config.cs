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

        public static void Load(string configContent)
        {
            ObservableCollection<Site> Sites = new ObservableCollection<Site>();
            JsonArray jsonValues = JsonValue.Parse(configContent).GetArray();
            foreach (JsonValue jsonObject in jsonValues)
            {
                Sites.Add(
                    new Site(jsonObject.GetObject())
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

        // преобразование в строку и форматированную строку

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

            return "[" + String.Join(",", sites.ToArray()) + "]";
        }

        public static string AsFormatString()
        {
            List<string> sites = new List<string>();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            foreach (string key in localSettings.Values.Keys)
            {
                if (!key.Contains("site_"))
                {
                    continue;
                }
                sites.Add(FormatJson(localSettings.Values[key].ToString()));
            }

            return String.Join("\n", sites.ToArray());
        }

        private const string INDENT_STRING = "    ";

        static string FormatJson(string json)
        {
            List<string> props = new List<string>();
            JsonObject jsonObject = JsonValue.Parse(json).GetObject();
            foreach (string key in new string[] {
                "name",
                "search_link",
                "search_post",
                "search_regexp",
                "chapters_link",
                "chapters_regexp",
                "pages_link",
                "pages_regexp"
            })
            {
                props.Add(INDENT_STRING + key + ":   " + jsonObject.GetNamedString(key));
            }
            return "{\n" + String.Join("\n", props.ToArray()) + "\n}";
        }
    }
}
