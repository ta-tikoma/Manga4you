using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Manga.Models
{
    enum SearchMangaType
    {
        ALREADY_ADDED,
        NOT_ADDED,
        NOW_ADDED
    }

    class SearchManga : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name)
        {
            //System.Diagnostics.Debug.WriteLine("RaiseProperty:" + name);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private SearchMangaType _is_added = SearchMangaType.NOT_ADDED;
        public Symbol is_added
        {
            get {
                switch (_is_added)
                {
                    case SearchMangaType.ALREADY_ADDED:
                        return Symbol.Accept;
                    case SearchMangaType.NOT_ADDED:
                        return Symbol.Add;
                    case SearchMangaType.NOW_ADDED:
                        return Symbol.Remove;
                }
                return Symbol.Add;
            }
        }

        public string name { get; set; } = null;
        public string link = null;
        public string site_hash = null;

        public SearchManga(string name, string link, string site_hash, ObservableCollection<Manga> History)
        {
            this.name = name;
            this.link = link;
            this.site_hash = site_hash;

            if (History.Any(m => Compare(m)))
            {
                _is_added = SearchMangaType.ALREADY_ADDED;
            }
            RaiseProperty("is_added");
        }

        public bool Compare(Manga manga)
        {
            return (manga.site_hash == site_hash)
                &&
                (manga.link == link)
                ;
        }

        public SearchMangaType Toggle(ref ObservableCollection<Manga> History)
        {
            if (_is_added == SearchMangaType.NOW_ADDED)
            {
                History.Remove(History.Single(m => Compare(m)));
                _is_added = SearchMangaType.NOT_ADDED;
            } else if (_is_added == SearchMangaType.NOT_ADDED)
            {
                History.Insert(0, new Manga()
                {
                    name = name,
                    link = link,
                    site_hash = site_hash
                });
                _is_added = SearchMangaType.NOW_ADDED;
            }
            RaiseProperty("is_added");
            return _is_added;
        }
    }
}
