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
        private SlidableListItem sli = null;

        private void SlidableListItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Menu_Show(
                sender as SlidableListItem,
                e.GetPosition(sender as UIElement)
            );
        }
        private void SlidableListItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Menu_Show(
                sender as SlidableListItem,
                e.GetPosition(sender as UIElement)
            );
        }
        private void Menu_Show(SlidableListItem slidableListItem, Point point)
        {
            if (menu_is_show)
            {
                return;
            }
            sli = slidableListItem;
            ChapterMenu.ShowAt(sli, point);
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
        private void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            Save();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private async Task Save()  
        {
            Ring.IsActive = true;
            Models.Chapter chapter = (VisualTreeHelper.GetParent(sli) as ListViewItemPresenter).DataContext as Models.Chapter;
            await chapter.Download(ExampleInAppNotification);
            Ring.IsActive = false;
        }

        // ссылка на главу
        private void CopyChapterLink_Click(object sender, RoutedEventArgs e)
        {
            Models.Chapter chapter = (VisualTreeHelper.GetParent(sli) as ListViewItemPresenter).DataContext as Models.Chapter;
            KeyValuePair<string, bool> LinkScuccess = chapter.MakeLink();
            if (!LinkScuccess.Value)
            {
                return;
            }

            Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(LinkScuccess.Key);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ExampleInAppNotification.Show(resourceLoader.GetString("LinkIsCopyied"), 4000);
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
