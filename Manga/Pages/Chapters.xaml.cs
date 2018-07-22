using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Manga.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Chapters : Page
    {
        Models.Manga Manga = null;

        public Chapters()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            Manga = new Models.Manga(JsonValue.Parse(localSettings.Values["manga_0"].ToString()).GetObject(), 0);

            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        // загрузка главы
        private async void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                SlidableListItem sli = sender as SlidableListItem;
                Models.Chapter chapter = (VisualTreeHelper.GetParent(sli) as ListViewItemPresenter).DataContext as Models.Chapter;
                ExampleInAppNotification.Show(resourceLoader.GetString("folder_select"), 4000);
                Ring.IsActive = true;

                KeyValuePair<string, ObservableCollection<Models.Page>> res = await chapter.PagesLoad();
                if (res.Key != null)
                {
                    Ring.IsActive = false;
                    ExampleInAppNotification.Show(res.Key, 4000);
                    return;
                }

                int index = 1;
                foreach (Models.Page page in res.Value)
                {
                    string type = ".png";
                    if (page.image_url.IndexOf(".jpg") != -1)
                    {
                        type = ".jpg";
                    }

                    if (page.image_url.IndexOf(".jpeg") != -1)
                    {
                        type = ".jpeg";
                    }

                    if (page.image_url.IndexOf(".gif") != -1)
                    {
                        type = ".gif";
                    }

                    if (page.image_url.IndexOf(".bmp") != -1)
                    {
                        type = ".bmp";
                    }

                    StorageFile sampleFile = await folder.CreateFileAsync(index + type, CreationCollisionOption.ReplaceExisting);
                    var cli = new HttpClient();
                    Byte[] bytes = await cli.GetByteArrayAsync(page.image_url);
                    IBuffer buffer = bytes.AsBuffer();
                    await FileIO.WriteBufferAsync(sampleFile, buffer);
                    ExampleInAppNotification.Show(resourceLoader.GetString(index + "/" + res.Value.Count()), 2000);
                    index++;
                }
                ExampleInAppNotification.Show(resourceLoader.GetString("download_complit"), 4000);
                Ring.IsActive = false;
            }
            else
            {
                ExampleInAppNotification.Show(resourceLoader.GetString("folder_not_select"), 4000);
            }
        }

        // on click - open pages
        private async void MangaChapters_TappedAsync(object sender, TappedRoutedEventArgs e)
        {
            await Pages.OpenPages(this);
        }

        private void MangaChapters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MangaChapters.SelectedItem != null)
            {
                MangaChapters.ScrollIntoView(MangaChapters.SelectedItem);
            }
        }
    }
}
