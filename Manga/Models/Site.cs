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
    class Site : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string hash = "";
        public const string SITE_HACH_ARCHIVE = "ARCHIVE";

        public string _name = "Добавить сайт";
        public string name
        {
            get { return _name; }
            set { _name = value; RaiseProperty("name"); }
        }

        public string _search_link = "";
        public string search_link
        {
            get { return _search_link; }
            set { _search_link = value; RaiseProperty("search_link"); }
        }

        public string _search_post = "";
        public string search_post
        {
            get { return _search_post; }
            set { _search_post = value; RaiseProperty("search_post"); }
        }

        public string _search_regexp = "";
        public string search_regexp
        {
            get { return _search_regexp; }
            set { _search_regexp = value; RaiseProperty("search_regexp"); }
        }

        public string _chapters_link = "";
        public string chapters_link
        {
            get { return _chapters_link; }
            set { _chapters_link = value; RaiseProperty("chapters_link"); }
        }

        public string _chapters_regexp = "";
        public string chapters_regexp
        {
            get { return _chapters_regexp; }
            set { _chapters_regexp = value; RaiseProperty("chapters_regexp"); }
        }

        public string _pages_link = "";
        public string pages_link
        {
            get { return _pages_link; }
            set { _pages_link = value; RaiseProperty("pages_link"); }
        }

        public string _pages_regexp = "";
        public string pages_regexp
        {
            get { return _pages_regexp; }
            set { _pages_regexp = value; RaiseProperty("pages_regexp"); }
        }

        public Site(JsonObject jo)
        {
            if (jo.ContainsKey("name"))
            {
                name = jo.GetNamedString("name");
            }

            if (jo.ContainsKey("search_link"))
            {
                search_link = jo.GetNamedString("search_link");
            }

            if (jo.ContainsKey("search_post"))
            {
                search_post = jo.GetNamedString("search_post");
            }

            if (jo.ContainsKey("search_regexp"))
            {
                search_regexp = jo.GetNamedString("search_regexp");
            }

            if (jo.ContainsKey("chapters_link"))
            {
                chapters_link = jo.GetNamedString("chapters_link");
            }

            if (jo.ContainsKey("chapters_regexp"))
            {
                chapters_regexp = jo.GetNamedString("chapters_regexp");
            }

            if (jo.ContainsKey("pages_link"))
            {
                pages_link = jo.GetNamedString("pages_link");
            }

            if (jo.ContainsKey("pages_regexp"))
            {
                pages_regexp = jo.GetNamedString("pages_regexp");
            }

            hash = Helpers.Any.CreateMD5(name);
        }

        public void Save()
        {
            JsonObject jo = new JsonObject();
            jo.SetNamedValue("name", JsonValue.CreateStringValue(name));
            jo.SetNamedValue("search_link", JsonValue.CreateStringValue(search_link));
            jo.SetNamedValue("search_post", JsonValue.CreateStringValue(search_post));
            jo.SetNamedValue("search_regexp", JsonValue.CreateStringValue(search_regexp));
            jo.SetNamedValue("chapters_link", JsonValue.CreateStringValue(chapters_link));
            jo.SetNamedValue("chapters_regexp", JsonValue.CreateStringValue(chapters_regexp));
            jo.SetNamedValue("pages_link", JsonValue.CreateStringValue(pages_link));
            jo.SetNamedValue("pages_regexp", JsonValue.CreateStringValue(pages_regexp));

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["site_" + hash] = jo.ToString();
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

        /*
        public string TryGetLink(string link)
        {
            try
            {
                Uri url = new Uri(link);
                Uri url_site = new Uri(search_link);
                if (url.Host != url_site.Host)
                {
                    return "";
                } else
                {
                    string[] url_parts = link.Split(new string[] { url.Host }, StringSplitOptions.None);
                    return url_parts[1];
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        */

        public static Site GetByHash(string hash)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("site_" + hash))
            {
                return new Site(JsonValue.Parse(localSettings.Values["site_" + hash].ToString()).GetObject());
            }
            return null;
        }

        public static void SaveList(ObservableCollection<Site> Sites)
        {
            foreach (Site site in Sites)
            {
                site.Save();
            }
        }

        public static void LoadList(ref ObservableCollection<Site> Sites)
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
