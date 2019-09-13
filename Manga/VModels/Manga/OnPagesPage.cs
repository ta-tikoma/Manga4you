using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace Manga.VModels.Manga
{
    class OnPagesPage : OnChaptersPage
    {
        // Pages
        private ObservableCollection<Models.Page> _pages = new ObservableCollection<Models.Page>();
        public ObservableCollection<Models.Page> Pages
        {
            get
            {
                System.Diagnostics.Debug.WriteLine("OnPagesPage:Pages");
                if (_pages.Count() == 0)
                {
                    PagesLoad();
                }
                return _pages;
            }
        }

        private async Task PagesLoad()
        {
            if (isBusy)
            {
                return;
            }
            isBusy = true;

            // загружаем главы
            if (Chapters.Count() == 0)
            {
                _chapters = await Manga.ChaptersLoad();
            }

            // текущая глава
            Models.Chapter chapter = Chapters[Manga.CurrentChapter];

            // загружаем страницы
            _pages = await Chapters[Manga.CurrentChapter].PagesLoad();

            // сохраняем количество страниц
            Manga.PagesCount = _pages.Count;
            Helpers.Save.Mangas.SaveFirst(Manga);

            // добавляем страницу заглушку
            _pages.Add(new Models.Page()
            {
                number = Models.Page.NEXT_CHAPTER
            });

            isBusy = false;
            RaiseProperty("Pages");
            RaiseProperty("Manga");
        }

        public int CurrentPage
        {
            get
            {
                return Manga.CurrentPage;
            }
            set
            {
                Manga.CurrentPage = value;
                Helpers.Save.Mangas.SaveFirst(Manga);
            }
        }

        public async Task NextChapter()
        {
            Manga.CurrentChapter += 1;
            Manga.CurrentPage = 0;
            Helpers.Save.Mangas.SaveFirst(Manga);
            await PagesLoad();
        }

        // Zoom
        public void SetZoom(float zoom)
        {
            Manga.Zoom = zoom;
            Helpers.Save.Mangas.SaveFirst(Manga);
        }

        public void ToggleAutoZoom()
        {
            Manga.AutoZoom = !Manga.AutoZoom;
            Helpers.Save.Mangas.SaveFirst(Manga);
        }
    }
}
