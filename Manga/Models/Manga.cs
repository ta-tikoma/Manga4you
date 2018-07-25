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
        public const string FAVORIT = "";
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
            if (name != "symbols")
            {
                Save();
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string site_hash { get; set; } = "";
        public string name { get; set; } = "";
        public string link { get; set; } = "";
        public bool is_favorit { get; set; } = false;

        private float _zoom = 1;
        public float zoom
        {
            get { return _zoom; }
            set {
                if (_zoom == value)
                {
                    return;
                }

                _zoom = value;
                RaiseProperty("zoom");
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
                return _current_chapter;
            }
            set {
                if (chapters.Count() > 0)
                {
                    if (_current_chapter != value)
                    {
                        _current_page = 0;
                        _current_chapter = value;
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
            
            get { System.Diagnostics.Debug.WriteLine("get:_current_page:" + _current_page); return _current_page; }
            set {
                if (pages.Count() > 0)
                {
                    _current_page = value;
                    System.Diagnostics.Debug.WriteLine("set:_current_page:" + _current_page);
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
                System.Diagnostics.Debug.WriteLine("chapters:get");
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

            if (jo.ContainsKey("is_favorit"))
            {
                is_favorit = jo.GetNamedBoolean("is_favorit");
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

        public bool ToggleFavorit()
        {
            System.Diagnostics.Debug.WriteLine("ToggleFavorit:");
            is_favorit = !is_favorit;
            UpdateSymbolIcon();
            RaiseProperty("is_favorit");
            return is_favorit;
        }

        public void UpdateSymbolIcon()
        {
            System.Diagnostics.Debug.WriteLine("UpdateSymbolIcon");
            _symbols = "";
            List<string> symbols_list = new List<string>();

            if (is_favorit)
            {
                symbols_list.Add(FAVORIT);
            }

            if (_current_chapter == _chapters_count)
            {
                if (_chapters_count != 0)
                {
                   symbols_list.Add(COMPLITE);
                }
            }

            if (site_hash == Site.SITE_HACH_ARCHIVE)
            {
                symbols_list.Add(ARCHIVE);
            }
            _symbols = String.Join(" ", symbols_list.ToArray());
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

        public void Save()
        {
            System.Diagnostics.Debug.WriteLine("Save:" + index_for_save);

            JsonObject jo = new JsonObject();
            jo.SetNamedValue("name", JsonValue.CreateStringValue(name));
            jo.SetNamedValue("link", JsonValue.CreateStringValue(link));
            jo.SetNamedValue("is_favorit", JsonValue.CreateBooleanValue(is_favorit));
            jo.SetNamedValue("site_hash", JsonValue.CreateStringValue(site_hash));
            jo.SetNamedValue("zoom", JsonValue.CreateNumberValue(zoom));
            jo.SetNamedValue("chapters_count", JsonValue.CreateNumberValue(_chapters_count));
            jo.SetNamedValue("current_chapter", JsonValue.CreateNumberValue(_current_chapter));
            jo.SetNamedValue("pages_count", JsonValue.CreateNumberValue(_pages_count));
            jo.SetNamedValue("current_page", JsonValue.CreateNumberValue(_current_page));

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["manga_" + index_for_save] = jo.ToString();
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
            Site site = Site.GetByHash(site_hash);
            if (site == null)
            {
                return null;
            }

            if (site.chapters_link.Trim().Length == 0)
            {
                return null;
            }

            return new Uri(site.chapters_link.Replace("#link#", link));
        }

        public bool IsArchive()
        {
            return site_hash == Site.SITE_HACH_ARCHIVE;
        }

        bool pages_is_loading = false;
        private async Task<string> PagesLoad()
        {
            if (pages_is_loading)
            {
                return null;
            }
            pages_is_loading = true;

            KeyValuePair<string, ObservableCollection<Page>> pages_with_message = await chapters[_current_chapter].PagesLoad();
            if (pages_with_message.Key != null)
            {
                pages_is_loading = false;
                return pages_with_message.Key;
            }
            _pages.Clear();
            foreach (Models.Page page in pages_with_message.Value)
            {
                _pages.Add(page);
            }
            _pages_count = _pages.Count();
            
            RaiseProperty("current_page");
            RaiseProperty("pages_count");
            pages_is_loading = false;
            return null;
        }

        bool chapters_is_loading = false;
        private async Task<string> ChaptersLoad()
        {
            System.Diagnostics.Debug.WriteLine("ChaptersLoad:");
            if (chapters_is_loading)
            {
                return null;
            }
            chapters_is_loading = true;

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
            } else
            {
                // from web
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                Site site = Site.GetByHash(site_hash);
                if (site == null)
                {
                    return resourceLoader.GetString("not_found");
                }

                if (site.chapters_link.Trim().Length == 0)
                {
                    return resourceLoader.GetString("chapters_link");
                }

                if (site.chapters_regexp.Trim().Length == 0)
                {
                    return resourceLoader.GetString("chapters_regexp");
                }

                string res = await Helpers.Request.rh.Get(site.chapters_link.Replace("#link#", link));
                try
                {
                    Regex regex = new Regex(site.chapters_regexp);
                    MatchCollection matches = regex.Matches(res);
                    chapters.Clear();
                    foreach (Match match in matches)
                    {
                        string name = null;
                        string link = null;
                        GroupCollection collection = match.Groups;
                        for (int i = 0; i < collection.Count; i++)
                        {
                            Group group = collection[i];
                            if (regex.GroupNameFromNumber(i) == "name")
                            {
                                name = Regex.Unescape(group.Value).Trim();
                                name = Regex.Replace(name, @"\t|\n|\r", "");
                                name = Regex.Replace(name, @"  ", "");
                            }
                            if (regex.GroupNameFromNumber(i) == "link")
                            {
                                link = group.Value;
                            }
                        }

                        if ((name != null) && (link != null))
                        {
                            chapters.Insert(0, new Chapter() { name = name, link = link, site_hash = site_hash });
                        }
                    }

                    _chapters_count = chapters.Count;
                    RaiseProperty("chapters_count");
                    RaiseProperty("current_chapter");
                }
                catch (Exception e)
                {
                    chapters_is_loading = false;
                    return resourceLoader.GetString("mask_error") + ": " + e.Message;
                }
            }

            if (pages_need_load)
            {
                chapters_is_loading = false;
                return await PagesLoad();
            }

            chapters_is_loading = false;
            return null;
        }

        public async Task<string> Refresh()
        {
            if (IsArchive())
            {
                return null;
            }

            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            Site site = Site.GetByHash(site_hash);
            if (site == null)
            {
                return resourceLoader.GetString("not_found");
            }

            if (site.chapters_link.Trim().Length == 0)
            {
                return resourceLoader.GetString("chapters_link");
            }

            if (site.chapters_regexp.Trim().Length == 0)
            {
                return resourceLoader.GetString("chapters_regexp");
            }

            string res = await Helpers.Request.rh.Get(site.chapters_link.Replace("#link#", link));
            try
            {
                Regex regex = new Regex(site.chapters_regexp);
                MatchCollection matches = regex.Matches(res);
                int count = 0;
                foreach (Match match in matches)
                {
                    string name = null;
                    string link = null;
                    GroupCollection collection = match.Groups;
                    for (int i = 0; i < collection.Count; i++)
                    {
                        Group group = collection[i];
                        if (regex.GroupNameFromNumber(i) == "name")
                        {
                            name = Regex.Unescape(group.Value).Trim();
                            name = Regex.Replace(name, @"\t|\n|\r", "");
                            name = Regex.Replace(name, @"  ", "");
                        }
                        if (regex.GroupNameFromNumber(i) == "link")
                        {
                            link = group.Value;
                        }
                    }

                    if ((name != null) && (link != null))
                    {
                        count++;
                    }
                }

                _chapters_count = count;
                RaiseProperty("chapters_count");
                RaiseProperty("items_count");
            }
            catch (Exception e)
            {
                return resourceLoader.GetString("mask_error") + ": " + e.Message;
            }

            return null;
        }

        // manga list save and load

        public static void SaveList(ObservableCollection<Manga> MangaList)
        {
            int index = 0;
            for (; index < MangaList.Count; index++)
            {
                MangaList[index].index_for_save = index;
            }

            // remove old
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            while (localSettings.Values.ContainsKey("manga_" + index))
            {
                localSettings.Values.Remove("manga_" + index);
                index++;
            }
        }

        public static void LoadList(ref ObservableCollection<Manga> MangaList)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            int index = 0;
            while (localSettings.Values.ContainsKey("manga_" + index))
            {
                MangaList.Add(
                    new Manga(
                        JsonValue.Parse(localSettings.Values["manga_" + index].ToString()).GetObject(),
                        index
                        )
                    );
                index++;
            }
        }
    }
}
