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
        public string Name { get; set; } = "";
        public string Link { get; set; } = "";
        public string SiteHash { get; set; } = "";

        public async Task<ObservableCollection<Page>> PagesLoad()
        {
            Site site = Site.GetByHash(SiteHash);

            string res = await Helpers.Cache.giveMeString(Link);

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

        // DOWNLOAD 
        public async Task Download(Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification ExampleInAppNotification)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("Archive", new List<string>() { ".zip" });
            savePicker.SuggestedFileName = Name;
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
