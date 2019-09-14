using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

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

        public string Name
        {
            get { return Manga.Name.ToUpper(); }
        }

        private string _symbols = null;
        public string Symbols
        {
            get { return _symbols; }
        }

        private BitmapImage _current_image = null;
        public BitmapImage CurrentImage
        {
            get {
                if (_current_image == null)
                {
                    LoadCurrentImage();
                }
                return _current_image;
            }
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

        private async Task LoadCurrentImage()
        {
            if (isBusy)
            {
                return;
            }
            isBusy = true;

            // загружаем главы
            ObservableCollection<Models.Chapter> chapters = await Manga.ChaptersLoad();

            // текущая глава
            Models.Chapter chapter = chapters[Manga.CurrentChapter];

            // загружаем страницы
            ObservableCollection<Models.Page> pages = await chapters[Manga.CurrentChapter].PagesLoad();

            Models.Page page = pages[Manga.CurrentPage];

            await page.LoadImageSite();

            _current_image = page.image;

            RaiseProperty("CurrentImage");
            isBusy = false;
        }

        public async Task Refresh()
        {
            await Manga.ChaptersLoad(true);
            UpdateSymbolIcon();
            RaiseProperty("Manga");
        }
    }
}
