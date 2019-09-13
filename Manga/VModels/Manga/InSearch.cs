using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Manga.VModels.Manga
{
    enum SearchMangaType
    {
        ALREADY_ADDED,  // давно в списке
        NOT_ADDED,      // не добавлен
        NOW_ADDED       // только что добавлен
    }

    class InSearch : VManga
    {
        // иконка добавления
        private SearchMangaType _is_added = SearchMangaType.NOT_ADDED;
        public Symbol is_added
        {
            get
            {
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

        // конструктор с проверкой наличия в списке
        public InSearch(Models.Manga manga, ObservableCollection<InHistory> history)
        {
            this.Manga = manga;

            if (history.Any(inHistory => manga.Compare(inHistory.Manga)))
            {
                _is_added = SearchMangaType.ALREADY_ADDED;
            }
            RaiseProperty("is_added");
        }

        // добавление\удаление в историю
        public SearchMangaType Toggle(ref ObservableCollection<InHistory> History)
        {
            switch (_is_added)
            {
                case SearchMangaType.ALREADY_ADDED:
                    break;
                case SearchMangaType.NOT_ADDED:
                    History.Insert(0, new InHistory(Manga));
                    _is_added = SearchMangaType.NOW_ADDED;
                    break;
                case SearchMangaType.NOW_ADDED:
                    History.Remove(History.Single(InHistory => Manga.Compare(InHistory.Manga)));
                    _is_added = SearchMangaType.NOT_ADDED;
                    break;
                default:
                    break;
            }
            
            RaiseProperty("is_added");
            return _is_added;
        }
    }
}
