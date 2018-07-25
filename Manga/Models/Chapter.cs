using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace Manga.Models
{
    class Chapter
    {
        public string name { get; set; } = "";
        public string link { get; set; } = "";
        public string site_hash { get; set; } = "";

        private async Task<KeyValuePair<string, ObservableCollection<Page>>> PagesLoadArchive()
        {
            ObservableCollection<Page> pages = new ObservableCollection<Page>();

            StorageFolder folder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(link);
            if (folder == null)
            {
                return new KeyValuePair<string, ObservableCollection<Page>>("Folder not found", pages);
            }

            int number = 0;

            foreach (var file in await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName))
            {
                if (file.ContentType.IndexOf("image") == -1)
                {
                    continue;
                }
                number++;
                pages.Add(new Page()
                {
                    image_url = link + "\\" + file.Name,
                    number = number.ToString()
                });
            }

            return new KeyValuePair<string, ObservableCollection<Page>>(null, pages);
        }

        private async Task<KeyValuePair<string, ObservableCollection<Page>>> PagesLoadSite()
        {
            ObservableCollection<Page> pages = new ObservableCollection<Page>();
            Site site = Site.GetByHash(site_hash);
            if (site == null)
            {
                return new KeyValuePair<string, ObservableCollection<Page>>("Привязанный сайт не обнаружен.", pages);
            }

            if (site.pages_link.Trim().Length == 0)
            {
                return new KeyValuePair<string, ObservableCollection<Page>>("Ссылка для глав не указана у сайта.", pages);
            }

            if (site.pages_regexp.Trim().Length == 0)
            {
                return new KeyValuePair<string, ObservableCollection<Page>>("Маска для глав не указана у сайта.", pages);
            }

            string res = await Helpers.Request.rh.Get(site.pages_link.Replace("#link#", link));

            try
            {
                Regex regex = new Regex(site.pages_regexp);
                MatchCollection matches = regex.Matches(res);
                int number = 0;
                foreach (Match match in matches)
                {
                    string link = "";
                    GroupCollection collection = match.Groups;
                    for (int i = 1; i < collection.Count; i++)
                    {
                        link += collection[i].Value;
                    }

                    if ((link != ""))
                    {
                        number++;
                        pages.Add(new Page() { image_url = link, number = number.ToString() });
                    }
                }
            }
            catch (Exception e)
            {
                return new KeyValuePair<string, ObservableCollection<Page>>("Ошибка маски: " + e.Message, pages);
            }

            return new KeyValuePair<string, ObservableCollection<Page>>(null, pages);
        }

        public async Task<KeyValuePair<string, ObservableCollection<Page>>> PagesLoad()
        {
            System.Diagnostics.Debug.WriteLine("PagesLoad:PagesLoad");
            if (site_hash == Site.SITE_HACH_ARCHIVE)
            {
                System.Diagnostics.Debug.WriteLine("PagesLoad:PagesLoadArchive");
                return await PagesLoadArchive();
            }

            System.Diagnostics.Debug.WriteLine("PagesLoad:PagesLoadSite");
            return await PagesLoadSite();
        }
    }
}
