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

        // меню | menu
        private bool menu_is_show = false;
        private Models.Chapter chapter = null;

        private void MangaChapters_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ListView listView = (ListView)sender;
            Menu_Show(
                ((FrameworkElement)e.OriginalSource).DataContext as Models.Chapter,
                listView,
                e.GetPosition(listView)
            );
        }
        private void MangaChapters_Holding(object sender, HoldingRoutedEventArgs e)
        {
            ListView listView = (ListView)sender;
            Menu_Show(
                ((FrameworkElement)e.OriginalSource).DataContext as Models.Chapter,
                listView,
                e.GetPosition(listView)
            );
        }
        private void Menu_Show(Models.Chapter _chapter, ListView listView, Point point)
        {
            if (menu_is_show)
            {
                return;
            }
            chapter = _chapter;
            MangaChapters.SelectedItem = chapter;
            ChapterMenu.ShowAt(listView, point);
        }
        private void Menu_Opened(object sender, object e)
        {
            menu_is_show = true;
        }
        private void Menu_Closed(object sender, object e)
        {
            menu_is_show = false;
        }

        // загрузка главы
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            Ring.IsActive = true;
            await chapter.Download(ExampleInAppNotification);
            Ring.IsActive = false;
        }

        // ссылка на главу
        private void CopyChapterLink_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(chapter.link);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ExampleInAppNotification.Show(resourceLoader.GetString("LinkIsCopyied"), 4000);
        }

        // on click - open pages
        private async void MangaChapters_TappedAsync(object sender, TappedRoutedEventArgs e)
        {
            Manga.current_chapter = MangaChapters.SelectedIndex;
            await Pages.OpenPages(this);
        }
    }
}
