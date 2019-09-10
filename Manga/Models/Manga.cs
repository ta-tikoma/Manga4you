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
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Manga.Models
{
    class Manga : INotifyPropertyChanged
    {
        public const string LOCK = "";
        public const string COMPLITE = "";
        public const string ARCHIVE = "";

        private int _index_for_save = 0;
        public int index_for_save
        {
            get { return _index_for_save; }
            set { _index_for_save = value; RaiseProperty("index_for_save"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name)
        {
            if ((name != "symbols") && (name != "more") && (name != "is_load"))
            {
                Save();
            }

            //System.Diagnostics.Debug.WriteLine("RaiseProperty:" + name);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string site_hash { get; set; } = "";
        public string name { get; set; } = "";
        public string link { get; set; } = "";
        public bool is_lock { get; set; } = false;

        public string site
        {
            get {
                if (IsArchive())
                {
                    var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                    return resourceLoader.GetString("Archive");
                } else
                {
                    Site site = Site.GetByHash(site_hash);
                    return site.name;
                }
            }
        }

        private Windows.UI.Xaml.Visibility _more = Windows.UI.Xaml.Visibility.Collapsed;
        public Windows.UI.Xaml.Visibility more
        {
            get { return _more; }
            set
            {
                _more = value;
                RaiseProperty("more");
            }
        }

        private bool _is_load = true;
        public bool is_load
        {
            get { return _is_load; }
            set
            {
                _is_load = value;
                RaiseProperty("is_load");
            }
        }

        private float _zoom = 1;
        public float zoom
        {
            get { return _zoom; }
            set {
                //System.Diagnostics.Debug.WriteLine("_zoom:" + _zoom);
                if (_zoom == value)
                {
                    return;
                }

                _zoom = value;
                RaiseProperty("zoom");
            }
        }

        private bool _auto_zoom = true;
        public bool auto_zoom
        {
            get { return _auto_zoom; }
            set
            {
                if (_auto_zoom == value)
                {
                    return;
                }

                _auto_zoom = value;
                RaiseProperty("auto_zoom");
            }
        }

        private string _symbols = null;
        public string symbols
        {
            get { return _symbols; }
        }

        // chapters
        private int _chapters_count = 0;
        public string chapters_count
        {
            get {
                if (_chapters_count == 0)
                {
                    return "?";
                }
                return _chapters_count.ToString();
            }
        }

        private int _current_chapter = 0;
        public int current_chapter {
            get {
                //System.Diagnostics.Debug.WriteLine("current_chapter:get:" + _current_chapter);

                return _current_chapter;
            }
            set {
                //System.Diagnostics.Debug.WriteLine("current_chapter:set:" + value);
                if (chapters.Count() > 0)
                {
                    if (_current_chapter != value)
                    {
                        _current_page = 0;
                        _current_chapter = value;
                        RaiseProperty("current_page");
                        RaiseProperty("current_chapter");

                        // If chapter change in Pages page and need load pages of new chapter
                        if (pages_need_load)
                        {
                            PagesLoad();
                        }
                    }
                }
            }
        }

        // pages
        private int _pages_count = 0;
        public string pages_count
        {
            get
            {
                if (_pages_count == 0)
                {
                    return "?";
                }
                return _pages_count.ToString();
            }
        }

        private int _current_page = 0;
        public int current_page
        {
            
            get {
                //System.Diagnostics.Debug.WriteLine("get:_current_page:" + _current_page);
                return _current_page;
            }
            set {
                if (_pages.Count() > _current_page)
                {
                    _current_page = value;
                    //System.Diagnostics.Debug.WriteLine("set:_current_page:" + _current_page + ":" + _pages.Count());
                }
                RaiseProperty("current_page");
            }
        }

        // items (for main page)
        public string items_count
        {
            get
            {
                if (IsArchive())
                {
                    return pages_count;
                } else
                {
                    return chapters_count;
                }
            }
        }

        public int current_item
        {
            get
            {
                if (IsArchive())
                {
                    return current_page;
                }
                else
                {
                    return current_chapter;
                }
            }
        }

        private bool pages_need_load = false;

        private ObservableCollection<Chapter> _chapters = new ObservableCollection<Chapter>();
        public ObservableCollection<Chapter> chapters
        {
            get {
                //System.Diagnostics.Debug.WriteLine("chapters:get");
                if (_chapters.Count() == 0)
                {
                    ChaptersLoad();
                }
                return _chapters;
            }
        }

        public ObservableCollection<Page> _pages = new ObservableCollection<Page>();
        public ObservableCollection<Page> pages
        {
            get
            {
                System.Diagnostics.Debug.WriteLine("pages:get");
                if (_chapters.Count() == 0)
                {
                    pages_need_load = true;
                    ChaptersLoad();
                }
                return _pages;
            }
        }

        public Manga()
        {
        }

        // create from archive
        public Manga(string archive_link)
        {
            site_hash = Site.SITE_HACH_ARCHIVE;
            name = archive_link.Split('\\').Last();
            link = name + DateTime.Now.ToString("MM.dd.yyyy-HH.mm.ss"); // folder name
            UpdateSymbolIcon();
        }

        // create from history
        public Manga(JsonObject jo, int index_for_save)
        {
            this._index_for_save = index_for_save;

            if (jo.ContainsKey("name"))
            {
                name = jo.GetNamedString("name");
            }

            if (jo.ContainsKey("link"))
            {
                link = jo.GetNamedString("link");
            }

            if (jo.ContainsKey("site_hash"))
            {
                site_hash = jo.GetNamedString("site_hash");
            }

            if (jo.ContainsKey("zoom"))
            {
                _zoom = (float)jo.GetNamedNumber("zoom");
            }

            if (jo.ContainsKey("auto_zoom"))
            {
                _auto_zoom = jo.GetNamedBoolean("auto_zoom");
            }

            if (jo.ContainsKey("is_lock"))
            {
                is_lock = jo.GetNamedBoolean("is_lock");
            }
            // for prev version
            if (jo.ContainsKey("is_favorit"))
            {
                is_lock = jo.GetNamedBoolean("is_favorit");
            }

            if (jo.ContainsKey("chapters_count"))
            {
                _chapters_count = (int) jo.GetNamedNumber("chapters_count");
            }

            if (jo.ContainsKey("current_chapter"))
            {
                _current_chapter = (int)jo.GetNamedNumber("current_chapter");
            }

            if (jo.ContainsKey("pages_count"))
            {
                _pages_count = (int)jo.GetNamedNumber("pages_count");
            }

            if (jo.ContainsKey("current_page"))
            {
                _current_page = (int)jo.GetNamedNumber("current_page");
            }

            UpdateSymbolIcon();
        }

        // Toggle lock flag return current value
        public bool ToggleLock()
        {
            System.Diagnostics.Debug.WriteLine("ToggleLock:");
            is_lock = !is_lock;
            UpdateSymbolIcon();
            RaiseProperty("is_lock");
            return is_lock;
        }

        // Update symbols line
        private void UpdateSymbolIcon()
        {
            _symbols = "";
            List<string> symbols_list = new List<string>();

            if (is_lock)
            {
               //symbols_list.Add(LOCK);
            }

            //System.Diagnostics.Debug.WriteLine("items_count:" + items_count);
            //System.Diagnostics.Debug.WriteLine("current_item:" + current_item);

            if (items_count == (current_item + 1).ToString())
            {
                symbols_list.Add(COMPLITE);
            }

            if (site_hash == Site.SITE_HACH_ARCHIVE)
            {
                symbols_list.Add(ARCHIVE);
            }
            _symbols = String.Join("", symbols_list.ToArray());
            //System.Diagnostics.Debug.WriteLine("_symbols:[" + _symbols + "]");
            RaiseProperty("symbols");
        }

        public bool Compare(Manga manga)
        {
            if (link != manga.link)
            {
                return false;
            }

            return true;
        }

        public string ToJson()
        {
            JsonObject jo = new JsonObject();
            jo.SetNamedValue("name", JsonValue.CreateStringValue(name));
            jo.SetNamedValue("link", JsonValue.CreateStringValue(link));
            jo.SetNamedValue("is_lock", JsonValue.CreateBooleanValue(is_lock));
            jo.SetNamedValue("site_hash", JsonValue.CreateStringValue(site_hash));
            jo.SetNamedValue("zoom", JsonValue.CreateNumberValue(zoom));
            jo.SetNamedValue("auto_zoom", JsonValue.CreateBooleanValue(auto_zoom));
            jo.SetNamedValue("chapters_count", JsonValue.CreateNumberValue(_chapters_count));
            jo.SetNamedValue("current_chapter", JsonValue.CreateNumberValue(_current_chapter));
            jo.SetNamedValue("pages_count", JsonValue.CreateNumberValue(_pages_count));
            jo.SetNamedValue("current_page", JsonValue.CreateNumberValue(_current_page));
            return jo.ToString();
        }

        public void Save()
        {
            //System.Diagnostics.Debug.WriteLine("Save:" + index_for_save);
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["manga_" + index_for_save] = ToJson();
        }

        public async Task<bool> Delete()
        {
            if (IsArchive())
            {
                IStorageItem item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(link);
                if (item != null)
                {
                    if (item is StorageFolder)
                    {
                        await (item as StorageFolder).DeleteAsync();
                    }
                }
            }

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("manga_" + index_for_save))
            {
                localSettings.Values.Remove("manga_" + index_for_save);
                return true;
            }

            return false;
        }

        public Uri GetLink()
        {
            return new Uri(link);
        }

        public bool IsArchive()
        {
            return site_hash == Site.SITE_HACH_ARCHIVE;
        }

        bool pages_is_loading = false;
        private async Task PagesLoad()
        {
            if (pages_is_loading)
            {
                return;
            }
            pages_is_loading = true;
            is_load = true;

            _pages = await chapters[_current_chapter].PagesLoad();
            _pages_count = _pages.Count();

            // add one for check last page of chapter
            _pages.Add(new Page()
            {
                number = Page.NEXT_CHAPTER
            });

            RaiseProperty("current_page");
            RaiseProperty("pages_count");
            UpdateSymbolIcon();
            pages_is_loading = false;
            is_load = false;
        }

        bool chapters_is_loading = false;
        private async Task ChaptersLoad()
        {
            //System.Diagnostics.Debug.WriteLine("ChaptersLoad:");
            if (chapters_is_loading)
            {
                return;
            }
            chapters_is_loading = true;
            is_load = true;

            if (IsArchive())
            {
                // archive
                System.Diagnostics.Debug.WriteLine("IsArchive:");
                chapters.Add(new Chapter()
                {
                    name = name,
                    link = link,
                    site_hash = site_hash
                });
                System.Diagnostics.Debug.WriteLine("new Chapter:" + chapters.Count());
            }
            else
            {
                // from web
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                Site site = Site.GetByHash(site_hash);

                //string res = await Helpers.Request.rh.Get(site.chapters_link.Replace("#link#", link));
                string res = await Helpers.Cache.giveMeString(link);

                List<string> links = Helpers.Regular.GetValuesByJO(
                    site.GetJsonObject(Site.JO_TYPE_CHAPTER, Site.JO_PATH_LINK),
                    res
                );

                List<string> names = Helpers.Regular.GetValuesByJO(
                    site.GetJsonObject(Site.JO_TYPE_CHAPTER, Site.JO_PATH_NAME),
                    res
                );

                for (int i = 0; i < links.Count; i++)
                {
                    chapters.Insert(0, new Chapter() { name = names[i], link = links[i], site_hash = site_hash });
                }

                _chapters_count = chapters.Count;
                RaiseProperty("chapters_count");
                RaiseProperty("current_chapter");
            }

            UpdateSymbolIcon();

            if (pages_need_load)
            {
                chapters_is_loading = false;
                await PagesLoad();
                return;
            }

            is_load = false;
            chapters_is_loading = false;
        }

        public async Task Refresh()
        {
            if (IsArchive())
            {
                return;
            }

            Site site = Site.GetByHash(site_hash);
            string res = await Helpers.Cache.giveMeString(link);

            List<string> links = Helpers.Regular.GetValuesByJO(
                site.GetJsonObject(Site.JO_TYPE_CHAPTER, Site.JO_PATH_LINK),
                res
            );

            List<string> names = Helpers.Regular.GetValuesByJO(
                site.GetJsonObject(Site.JO_TYPE_CHAPTER, Site.JO_PATH_NAME),
                res
            );

            for (int i = 0; i < links.Count; i++)
            {
                chapters.Insert(0, new Chapter() { name = names[i], link = links[i], site_hash = site_hash });
            }

            _chapters_count = chapters.Count;
            RaiseProperty("chapters_count");
            RaiseProperty("items_count");
        }
    }
}
