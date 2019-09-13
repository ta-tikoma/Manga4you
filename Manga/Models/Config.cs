using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Storage;

namespace Manga.Models
{
    class Config
    {
        public const string CONFIG_LINK = "https://raw.githubusercontent.com/ta-tikoma/Manga4you/master/Manga/Assets/sites-v3.json";

        public static async Task CheckAsync()
        {
            if (!NeedUpdate())
            {
                return;
            }

            await DownloadAsync();
        }

        public static string GetVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("v{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        public static bool NeedUpdate()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            string version = GetVersion();

            if (localSettings.Values.ContainsKey("date_last_update_" + version))
            {
                string date_last_update = localSettings.Values["date_last_update_" + version].ToString();
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
            ObservableCollection<Site> sites = new ObservableCollection<Site>();
            JsonArray jsonValues = JsonValue.Parse(configContent).GetArray();
            foreach (JsonValue jsonObject in jsonValues)
            {
                System.Diagnostics.Debug.WriteLine("jsonObject:" + jsonObject.ToString());
                sites.Add(
                    new Site(jsonObject.GetObject())
                );
            }

            Helpers.Save.Sites.Save(sites);

            DateTime currentDate = DateTime.Now;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string version = GetVersion();
            localSettings.Values["date_last_update_" + version] = currentDate.ToString();
        }

        public static async Task DownloadAsync()
        {
            //System.Diagnostics.Debug.WriteLine("SetDefaultSites");

            string res = await Helpers.Request.rh.Get(CONFIG_LINK);
            if (res == null)
            {
                //System.Diagnostics.Debug.WriteLine("(res == null)");
                return;
            }

            Load(res);
        }

        // преобразование в строку и форматированную строку
        /*
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

        public static List<JsonObject> AsListOfList()
        {
            List<JsonObject> sites = new List<JsonObject>();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            foreach (string key1 in localSettings.Values.Keys)
            {
                if (!key1.Contains("site_"))
                {
                    continue;
                }
                JsonObject jsonObject = ;
                sites.Add(
                    JsonValue.Parse(localSettings.Values[key1].ToString()).GetObject()
                );
            }

            return sites;
        }
        */
    }
}
