using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace Manga.Models
{
    class Site : INotifyPropertyChanged
    {
        public const string JO_TYPE_MANGA = "manga";
        public const string JO_TYPE_CHAPTER = "chapter";
        public const string JO_TYPE_PAGE = "page";
        public const string JO_TYPE_SEARCH = "serach";

        public const string JO_PATH_LINK = "link";
        public const string JO_PATH_NAME = "name";

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string hash = "";
        public const string SITE_HACH_ARCHIVE = "ARCHIVE";

        public string Name { get; set; }
        private JsonObject jo;

        public Site(JsonObject jo)
        {
            this.jo = jo;

            if (jo.ContainsKey("name"))
            {
                Name = jo.GetNamedString("name");
            }

            hash = Helpers.Any.CreateMD5(Name);
        }

        public JsonObject GetJsonObject(string type, string path = null)
        {
            if (path != null)
            {
                return jo.GetNamedObject(type).GetNamedObject(path);
            }

            return jo.GetNamedObject(type);
        }

        public void Save()
        {
            ApplicationData.Current.LocalSettings.Values["site_" + hash] = jo.ToString();
        }

        public bool Delete()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("site_" + hash))
            {
                localSettings.Values.Remove("site_" + hash);
                return true;
            }

            return false;
        }

        public static Site GetByHash(string hash)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("site_" + hash))
            {
                return new Site(JsonValue.Parse(localSettings.Values["site_" + hash].ToString()).GetObject());
            }
            return null;
        }
    }
}
