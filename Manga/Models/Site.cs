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

        public bool _is_enabled = true;
        public bool is_enabled
        {
            get { return _is_enabled; }
            set { _is_enabled = value; RaiseProperty("is_enabled"); }
        }

        public Site()
        {
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

            if (jo.ContainsKey("is_enabled"))
            {
                is_enabled = jo.GetNamedBoolean("is_enabled");
            }

            if (jo.ContainsKey("hash"))
            {
                hash = jo.GetNamedString("hash");
            }
        }

        public void Save()
        {
            if (hash == "")
            {
                hash = Helpers.Any.CreateMD5(name);
            }

            JsonObject jo = new JsonObject();
            jo.SetNamedValue("name", JsonValue.CreateStringValue(name));
            jo.SetNamedValue("search_link", JsonValue.CreateStringValue(search_link));
            jo.SetNamedValue("search_post", JsonValue.CreateStringValue(search_post));
            jo.SetNamedValue("search_regexp", JsonValue.CreateStringValue(search_regexp));
            jo.SetNamedValue("chapters_link", JsonValue.CreateStringValue(chapters_link));
            jo.SetNamedValue("chapters_regexp", JsonValue.CreateStringValue(chapters_regexp));
            jo.SetNamedValue("pages_link", JsonValue.CreateStringValue(pages_link));
            jo.SetNamedValue("pages_regexp", JsonValue.CreateStringValue(pages_regexp));
            jo.SetNamedValue("hash", JsonValue.CreateStringValue(hash));
            jo.SetNamedValue("is_enabled", JsonValue.CreateBooleanValue(is_enabled));

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

        public static void DeleteList(ref ObservableCollection<Site> Sites)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            Sites.Clear();
            foreach (string key in localSettings.Values.Keys)
            {
                if (!key.Contains("site_"))
                {
                    continue;
                }

                localSettings.Values.Remove(key);
            }
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

        public static async Task<bool> SetDefaultSites()
        {
            System.Diagnostics.Debug.WriteLine("SetDefaultSites");
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            string res = await Helpers.Request.rh.Get("https://raw.githubusercontent.com/ta-tikoma/Manga4you/master/config.txt");
            if (res == null)
            {
                System.Diagnostics.Debug.WriteLine("(res == null)");
                return false;
            }

            ObservableCollection<Site> site_list = new ObservableCollection<Site>();

            string[] sites = res.Split(new[] { " ## \n" }, StringSplitOptions.None);
            foreach (string site_string in sites)
            {
                Site site = new Site()
                {
                    is_enabled = false
                };

                string[] lines = site_string.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );

                if (lines[0].Length > 0)
                {
                    site.name = lines[0];
                    System.Diagnostics.Debug.WriteLine("name:'" + lines[0] + "'");
                }

                if (lines[1].Length > 0)
                {
                    site.search_link = lines[1];
                    System.Diagnostics.Debug.WriteLine("search_link:'" + lines[1] + "'");
                }

                if (lines[2].Length > 0)
                {
                    site.search_post = lines[2];
                    System.Diagnostics.Debug.WriteLine("search_post:'" + lines[2] + "'");
                }

                if (lines[3].Length > 0)
                {
                    site.search_regexp = lines[3];
                    System.Diagnostics.Debug.WriteLine("search_regexp:'" + lines[3] + "'");
                }

                if (lines[4].Length > 0)
                {
                    site.chapters_link = lines[4];
                    System.Diagnostics.Debug.WriteLine("chapters_link:'" + lines[4] + "'");
                }

                if (lines[5].Length > 0)
                {
                    site.chapters_regexp = lines[5];
                    System.Diagnostics.Debug.WriteLine("chapters_regexp:'" + lines[5] + "'");
                }

                if (lines[6].Length > 0)
                {
                    site.pages_link = lines[6];
                    System.Diagnostics.Debug.WriteLine("pages_link:'" + lines[6] + "'");
                }

                if (lines[7].Length > 0)
                {
                    site.pages_regexp = lines[7];
                    System.Diagnostics.Debug.WriteLine("pages_regexp:'" + lines[7] + "'");
                }

                site_list.Add(site);
            }

            if (site_list.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("(site_list.Count == 0)");
                return false;
            }

            SaveList(site_list);

            DateTime currentDate = DateTime.Now;
            localSettings.Values["date_last_update"] = currentDate.ToString();

            return true;
        }
    }
}
