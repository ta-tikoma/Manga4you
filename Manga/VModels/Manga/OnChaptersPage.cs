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
    class OnChaptersPage : VManga
    {
        protected ObservableCollection<Models.Chapter> _chapters = new ObservableCollection<Models.Chapter>();
        public ObservableCollection<Models.Chapter> Chapters
        {
            get
            {
                System.Diagnostics.Debug.WriteLine("OnChaptersPage:Chapters");
                if (_chapters.Count() == 0)
                {
                    ChaptersLoad();
                }
                return _chapters;
            }
        }

        protected bool isBusy = false;

        public OnChaptersPage()
        {
            System.Diagnostics.Debug.WriteLine("OnChaptersPage:");
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            this.Manga = new Models.Manga(
                JsonValue.Parse(localSettings.Values["manga_0"].ToString()).GetObject()
            );
            System.Diagnostics.Debug.WriteLine("OnChaptersPage:");
        }

        public void SetCurrentChapter(int currentChapter)
        {
            Manga.CurrentChapter = currentChapter;
            Helpers.Save.Mangas.SaveFirst(Manga);
        }

        public int CurrentChapter
        {
            get
            {
                return Manga.CurrentChapter;
            }
        }

        private async Task ChaptersLoad()
        {
            if (isBusy)
            {
                return;
            }
            isBusy = true;

            _chapters = await Manga.ChaptersLoad();

            isBusy = false;
            RaiseProperty("Chapters");
            RaiseProperty("Manga");
            RaiseProperty("CurrentChapter");
        }
    }
}
