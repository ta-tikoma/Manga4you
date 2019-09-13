using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.VModels.Manga
{
    class InHistory : VManga
    {
        private Windows.UI.Xaml.Visibility _more = Windows.UI.Xaml.Visibility.Collapsed;
        public Windows.UI.Xaml.Visibility More
        {
            get { return _more; }
            set
            {
                _more = value;
                RaiseProperty("More");
            }
        }

        private string _symbols = null;
        public string Symbols
        {
            get { return _symbols; }
        }

        public const string LOCK = "";
        public const string COMPLITE = "";
        public const string ARCHIVE = "";

        public InHistory(Models.Manga manga)
        {
            this.Manga = manga;
            UpdateSymbolIcon();
        }

        private void UpdateSymbolIcon()
        {
            _symbols = "";
            if (Manga.ChaptersCount == (Manga.CurrentChapter + 1))
            {
                _symbols += COMPLITE;
            }
            if (Manga.SiteHash == Models.Site.SITE_HACH_ARCHIVE)
            {
                _symbols += ARCHIVE;
            }
            RaiseProperty("Symbols");
        }

        public async Task Refresh()
        {
            await Manga.ChaptersLoad();
            RaiseProperty("Manga");
        }
    }
}
