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

        public KeyValuePair<string, bool> MakeLink()
        {
            Site site = Site.GetByHash(site_hash);
            if (site == null)
            {
                return new KeyValuePair<string, bool>("Привязанный сайт не обнаружен.", false);
            }

            if (site.pages_link.Trim().Length == 0)
            {
                return new KeyValuePair<string, bool>("Ссылка для глав не указана у сайта.", false);
            }

            if (site.pages_regexp.Trim().Length == 0)
            {
                return new KeyValuePair<string, bool>("Маска для глав не указана у сайта.", false);
            }

            return new KeyValuePair<string, bool>(site.pages_link.Replace("#link#", link), true);
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
            //System.Diagnostics.Debug.WriteLine("site.pages_link:" + site.pages_link.Replace("#link#", link));

            try
            {
                Regex regex = new Regex(site.pages_regexp);
                System.Diagnostics.Debug.WriteLine("site.pages_regexp:" + site.pages_regexp);
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
                //System.Diagnostics.Debug.WriteLine("pages.Count:" + pages.Count);
            }
            catch (Exception e)
            {
                return new KeyValuePair<string, ObservableCollection<Page>>("Ошибка маски: " + e.Message, pages);
            }

            return new KeyValuePair<string, ObservableCollection<Page>>(null, pages);
        }

        public async Task<KeyValuePair<string, ObservableCollection<Page>>> PagesLoad()
        {
            //System.Diagnostics.Debug.WriteLine("PagesLoad:PagesLoad");
            if (site_hash == Site.SITE_HACH_ARCHIVE)
            {
                //System.Diagnostics.Debug.WriteLine("PagesLoad:PagesLoadArchive");
                return await PagesLoadArchive();
            }

            //System.Diagnostics.Debug.WriteLine("PagesLoad:PagesLoadSite");
            return await PagesLoadSite();
        }

        public async Task Download2(Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification ExampleInAppNotification)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                ExampleInAppNotification.Show(resourceLoader.GetString("folder_select"), 4000);

                KeyValuePair<string, ObservableCollection<Models.Page>> res = await PagesLoad();
                if (res.Key != null)
                {
                    ExampleInAppNotification.Show(res.Key, 4000);
                    return;
                }

                int index = 1;
                foreach (Models.Page page in res.Value)
                {
                    await page.Download(folder);
                    ExampleInAppNotification.Show(resourceLoader.GetString(index + "/" + res.Value.Count()), 2000);
                    index++;
                }
                ExampleInAppNotification.Show(resourceLoader.GetString("download_complit"), 4000);
            }
            else
            {
                ExampleInAppNotification.Show(resourceLoader.GetString("folder_not_select"), 4000);
            }
        }

        public async Task Download(Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification ExampleInAppNotification)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("Archive", new List<string>() { ".zip" });
            savePicker.SuggestedFileName = name;
            StorageFile savefile = await savePicker.PickSaveFileAsync();
            if (savefile == null)
                return;

            string folder_name = "buffer_folder_" + DateTime.Now.ToString("yyyyMMddhhmmss");

            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folder_name);
            KeyValuePair<string, ObservableCollection<Page>> res = await PagesLoad();

            if (res.Key != null)
            {
                ExampleInAppNotification.Show(res.Key, 4000);
                return;
            }

            int index = 1;
            foreach (Page page in res.Value)
            {
                await page.Download(folder);
                ExampleInAppNotification.Show(index + "/" + res.Value.Count(), 2000);
                index++;
            }

            await Helpers.ZipArchiveManager.ZipFolder(folder, savefile);

            //ZipFile.CreateFromDirectory(folder.Path, savefile.Path);

            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ExampleInAppNotification.Show(resourceLoader.GetString("download_complit"), 4000);
        }
    }
}
