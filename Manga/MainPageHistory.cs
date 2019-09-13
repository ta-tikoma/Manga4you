using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Manga
{
    public sealed partial class MainPage : Page
    {
        // больше данных | more
        private void ToggleManga_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            VModels.Manga.InSearch searchManga = (sender as Button).DataContext as VModels.Manga.InSearch;
            switch (searchManga.Toggle(ref History))
            {
                case VModels.Manga.SearchMangaType.NOW_ADDED:
                    ExampleInAppNotification.Show(resourceLoader.GetString("add_to_history"), 4000);
                    break;
                case VModels.Manga.SearchMangaType.NOT_ADDED:
                    ExampleInAppNotification.Show(resourceLoader.GetString("remove_from_history"), 4000);
                    break;
            }
        }

        // детали | more
        private void ToggleMore(VModels.Manga.InHistory manga)
        {
            if (manga.More == Visibility.Visible)
            {
                manga.More = Visibility.Collapsed;
            }
            else
            {
                manga.More = Visibility.Visible;
            }
        }

        private void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            SlidableListItem sli = sender as SlidableListItem;
            VModels.Manga.InHistory manga = (VisualTreeHelper.GetParent(sli) as ListViewItemPresenter).DataContext as VModels.Manga.InHistory;
            ToggleMore(manga);
        }

        // обновить количество глав | update chapters count
        private async void SlidableListItem_RightCommandRequested(object sender, EventArgs e)
        {
            Ring.IsActive = true;
            SlidableListItem sli = sender as SlidableListItem;
            VModels.Manga.InHistory manga = (VisualTreeHelper.GetParent(sli) as ListViewItemPresenter).DataContext as VModels.Manga.InHistory;
            await manga.Refresh();
            Ring.IsActive = false;
        }

        // кнопки истории | history buttons
        private async void RefreshAll_ClickAsync(object sender, RoutedEventArgs e)
        {
            Ring.IsActive = true;
            foreach (VModels.Manga.InHistory manga in History)
            {
                await manga.Refresh();
            }
            Ring.IsActive = false;
        }

        // открыть страницы | open pages
        private void HistoryList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (HistoryList.SelectedItem == null)
            {
                return;
            }

            History.Move(HistoryList.SelectedIndex, 0);
            Pages.Pages.OpenPages(this);
        }

        // меню | menu
        private VModels.Manga.InHistory selected_manga = null;
        private bool menu_is_show = false;

        private void HistoryList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ListView listView = (ListView)sender;
            Menu_Show(
                ((FrameworkElement)e.OriginalSource).DataContext as VModels.Manga.InHistory,
                listView,
                e.GetPosition(listView)
            );
        }
        private void HistoryList_Holding(object sender, HoldingRoutedEventArgs e)
        {
            ListView listView = (ListView)sender;
            Menu_Show(
                ((FrameworkElement)e.OriginalSource).DataContext as VModels.Manga.InHistory,
                listView,
                e.GetPosition(listView)
            );
        }
        private void Menu_Show(VModels.Manga.InHistory manga, ListView listView, Point point)
        {
            if (menu_is_show)
            {
                return;
            }
            selected_manga = manga;
            //if (manga.IsArchive())
            //{
            //  MenuArchive.ShowAt(listView, point);
            //} else
            {
                MenuSite.ShowAt(listView, point);
            }
        }
        private void Menu_Opened(object sender, object e)
        {
            menu_is_show = true;
        }
        private void Menu_Closed(object sender, object e)
        {
            menu_is_show = false;
        }

        private void ToChapters_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            for (; i < History.Count; i++)
            {
                if (History[i] == selected_manga)
                {
                    break;
                }
            }
            History.Move(i, 0);
            this.Frame.Navigate(typeof(Pages.Chapters));
        }

        private async void Refresh_ClickAsync(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (selected_manga != null)
            {
                await selected_manga.Refresh();
                ExampleInAppNotification.Show(resourceLoader.GetString("chapters_updated"), 2000);
            }
        }

        private async void InBrowser_ClickAsync(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (selected_manga != null)
            {
                Uri uri = new Uri(selected_manga.Manga.Link);
                if (uri == null)
                {
                    ExampleInAppNotification.Show(resourceLoader.GetString("bad_link"), 2000);
                }
                else
                {
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                }
            }
        }

        private async void Delete_ClickAsync(object sender, RoutedEventArgs e)
        {
            Ring.IsActive = true;
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (selected_manga != null)
            {
                History.Remove(selected_manga);
            }
            Ring.IsActive = false;
        }

        private void Detail_Click(object sender, RoutedEventArgs e)
        {
            ToggleMore(selected_manga);
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "Export " + DateTime.Now.ToString("dd-MM-yyyy");

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                await Windows.Storage.FileIO.WriteTextAsync(file, Helpers.Save.Mangas.Export(History));
            }
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".txt");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string text = await Windows.Storage.FileIO.ReadTextAsync(file);
                Helpers.Save.Mangas.Import(ref History, text);
            }
        }
    }
}
