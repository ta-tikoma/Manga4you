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

        private async Task<ObservableCollection<Page>> PagesLoadArchive()
        {
            ObservableCollection<Page> pages = new ObservableCollection<Page>();

            StorageFolder folder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(link);
            if (folder == null)
            {
                return pages;
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

            return pages;
        }

        private async Task<ObservableCollection<Page>> PagesLoadSite()
        {
            Site site = Site.GetByHash(site_hash);

            string res = await Helpers.Cache.giveMeString(link);

            List<string> links = Helpers.Regular.GetValuesByJO(
                site.GetJsonObject(Site.JO_TYPE_PAGE, Site.JO_PATH_LINK),
                res
            );

            ObservableCollection<Page> pages = new ObservableCollection<Page>();
            for (int i = 0; i < links.Count; i++)
            {
                pages.Add(new Page()
                {
                    image_url = links[i],
                    number = (i + 1).ToString()
                });
            }
            //System.Diagnostics.Debug.WriteLine("pages.Count:" + pages.Count);
            return pages;
        }

        public async Task<ObservableCollection<Page>> PagesLoad()
        {
            try
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
            catch (Exception)
            {
                return null;
            }
        }

        

        // DOWNLOAD 
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

                ObservableCollection<Models.Page> pages = await PagesLoad();
                if (pages == null)
                {
                    ExampleInAppNotification.Show("Can't loading :(", 4000);
                    return;
                }

                int index = 1;
                foreach (Models.Page page in pages)
                {
                    await page.Download(folder);
                    ExampleInAppNotification.Show(resourceLoader.GetString(index + "/" + pages.Count()), 2000);
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
            ObservableCollection<Page> pages = await PagesLoad();

            if (pages == null)
            {
                ExampleInAppNotification.Show("Can't loading :(", 4000);
                return;
            }

            int index = 1;
            foreach (Page page in pages)
            {
                await page.Download(folder);
                ExampleInAppNotification.Show(index + "/" + pages.Count(), 2000);
                index++;
            }

            await Helpers.ZipArchiveManager.ZipFolder(folder, savefile);

            //ZipFile.CreateFromDirectory(folder.Path, savefile.Path);

            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ExampleInAppNotification.Show(resourceLoader.GetString("download_complit"), 4000);
        }
    }
}
