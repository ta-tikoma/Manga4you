﻿using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace Manga
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<Models.Site> Sites = new ObservableCollection<Models.Site>();
        ObservableCollection<Models.Manga> History = new ObservableCollection<Models.Manga>();

        public MainPage()
        {
            Models.Site.SetDefaultSites();
            Models.Manga.LoadList(ref History);
            Models.Site.LoadList(ref Sites);
            Helpers.Request.rh = new Helpers.Request();
            this.InitializeComponent();
            //System.Diagnostics.Debug.WriteLine("MasterDetailsView_ViewStateChanged:");
            HideStatusBarAsync();
            CheckSitesVersion();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private async Task CheckSitesVersion()
        {
            if (Models.Site.NeedUpdate())
            {
                Ring.IsActive = true;
                if (await Models.Site.SetDefaultSites())
                {
                    var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                    ExampleInAppNotification.Show(resourceLoader.GetString("config_update"), 4000);
                }
                Ring.IsActive = false;
            }
        }

        private async Task HideStatusBarAsync()
        {
            

            try
            {
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                await statusBar.HideAsync();

                /*var view = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                view.TryEnterFullScreenMode();*/
            }
            catch (Exception ex)
            {
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        // настройки | settings
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.Settings));
        }

        // поиск | search
        private async void Search_KeyUpAsync(object sender, KeyRoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (SiteSelect.SelectedItem == null)
                {
                    ExampleInAppNotification.Show(resourceLoader.GetString("select_site"), 2000);
                } else
                {
                    Models.Site site = SiteSelect.SelectedItem as Models.Site;
                    if (site.search_link.Trim().Length == 0)
                    {
                        ExampleInAppNotification.Show(resourceLoader.GetString("search_link_of_site"), 4000);
                        return;
                    }

                    if (site.search_regexp.Trim().Length == 0)
                    {
                        ExampleInAppNotification.Show(resourceLoader.GetString("mask_link_of_site"), 4000);
                        return;
                    }

                    Ring.IsActive = true;
                    string res = null;
                    if (site.search_post.Trim().Length == 0)
                    {
                        res = await Helpers.Request.rh.Get(site.search_link.Replace("#word#", Search.Text));
                    } else
                    {
                        res = await Helpers.Request.rh.Post(site.search_link, site.search_post.Replace("#word#", Search.Text));
                    }
                    if (res == null)
                    {
                        ExampleInAppNotification.Show(resourceLoader.GetString("request_failed"), 4000);
                        Ring.IsActive = false;
                        return;
                    }

                    ObservableCollection<Models.Manga> MangaList = new ObservableCollection<Models.Manga>();

                    try
                    {
                        Regex regex = new Regex(site.search_regexp);
                        MatchCollection matches = regex.Matches(res);
                        foreach (Match match in matches)
                        {
                            string name = null;
                            string link = null;
                            GroupCollection collection = match.Groups;
                            for (int i = 0; i < collection.Count; i++)
                            {
                                Group group = collection[i];
                                if (regex.GroupNameFromNumber(i) == "name")
                                {
                                    name = Regex.Replace(Regex.Unescape(group.Value), "<.*?>", String.Empty);
                                }
                                if (regex.GroupNameFromNumber(i) == "link")
                                {
                                    link = group.Value;
                                }
                            }

                            if ((name != null) && (link != null))
                            {
                                MangaList.Add(new Models.Manga() { name = name, link = link, site_hash = site.hash });
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        ExampleInAppNotification.Show(resourceLoader.GetString("mask_error") + ": " + exception.Message, 4000);
                        Ring.IsActive = false;
                        return;
                    }
                    

                    if (MangaList.Count() == 0)
                    {
                        ExampleInAppNotification.Show(resourceLoader.GetString("nothing_found"), 4000);
                        Ring.IsActive = false;
                        return;
                    }

                    SearchResultList.ItemsSource = MangaList;

                    SearchResult.Visibility = Visibility.Visible;
                    DeleteSearch.Focus(FocusState.Programmatic);
                    Ring.IsActive = false;
                }
            }
        }

        private void DeleteSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchResultList.ItemsSource = null;
            SearchResult.Visibility = Visibility.Collapsed;
        }

        private void SearchResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultList.SelectedItem != null)
            {
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                Models.Manga manga = SearchResultList.SelectedItem as Models.Manga;
                if (History.Any(m => m.Compare(manga)))
                {
                    ExampleInAppNotification.Show(resourceLoader.GetString("exist_in_history"), 4000);
                }
                else
                {
                    History.Insert(0, manga);
                    Models.Manga.SaveList(History);
                    ExampleInAppNotification.Show(resourceLoader.GetString("add_to_history"), 4000);
                }
            }
        }

        // история | history
        private void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            SlidableListItem sli = sender as SlidableListItem;
            Models.Manga manga = (VisualTreeHelper.GetParent(sli) as ListViewItemPresenter).DataContext as Models.Manga;
            manga.ToggleFavorit();
        }

        // обновить количество глав | update chapters count
        private void SlidableListItem_RightCommandRequested(object sender, EventArgs e)
        {
            SlidableListItem sli = sender as SlidableListItem;
            Models.Manga manga = (VisualTreeHelper.GetParent(sli) as ListViewItemPresenter).DataContext as Models.Manga;
            manga.Refresh();
        }

        // кнопки истории | history buttons
        private async void HistoryClear_ClickAsync(object sender, RoutedEventArgs e)
        {
            Ring.IsActive = true;
            int index = 0;
            while (index < History.Count)
            {
                if (History[index].is_favorit)
                {
                    index++;
                } else
                {
                    await History[index].Delete();
                    History.RemoveAt(index);
                }
            }

            Ring.IsActive = false;
        }

        private async void RefreshAll_ClickAsync(object sender, RoutedEventArgs e)
        {
            foreach (Models.Manga manga in History)
            {
                await manga.Refresh();
            }
        }

        private async void OpenArchive_ClickAsyn(object sender, RoutedEventArgs e)
        {
            Ring.IsActive = true;
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".zip");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                Models.Manga manga = new Models.Manga(file.Path);

                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                if (History.Any(m => m.Compare(manga)))
                {
                    ExampleInAppNotification.Show(resourceLoader.GetString("exist_in_history"), 4000);
                }
                else
                {
                    StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(manga.link);
                    await Helpers.ZipArchiveManager.UnZipFileAsync(file, folder, false);
                    History.Insert(0, manga);
                    Models.Manga.SaveList(History);
                    ExampleInAppNotification.Show(resourceLoader.GetString("add_to_history"), 4000);
                }
            }
            Ring.IsActive = false;
        }

        private void AddByLink_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            Grid grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBox link = new TextBox
            {
                PlaceholderText = resourceLoader.GetString("input_link"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 24, 0),
                FontSize = 16
            };
            grid.Children.Add(link);
            Grid.SetColumn(link, 0);

            var goButton = new Button
            {
                Content = new SymbolIcon(Symbol.Accept),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255))
            };

            goButton.Click += GoButton_Click;

            Grid.SetColumn(goButton, 1);
            grid.Children.Add(goButton);

            ExampleInAppNotification.Show(grid);
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            Button goButton = sender as Button;
            Grid grid = goButton.Parent as Grid;
            TextBox link = grid.Children.ElementAt(0) as TextBox;
            TextBox name = grid.Children.ElementAt(1) as TextBox;

            Models.Site current_site = null;
            foreach (Models.Site site in Sites)
            {
                string link_string = site.TryGetLink(link.Text);
                if (link_string != "")
                {
                    current_site = site;
                    break;
                }
            }
          
            Models.Manga manga = new Models.Manga() { name = name.Text, link = link.Text, site_hash = current_site.hash };
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (History.Any(m => m.Compare(manga)))
            {
                ExampleInAppNotification.Show(resourceLoader.GetString("exist_in_history"), 4000);
            }
            else
            {
                History.Insert(0, manga);
                Models.Manga.SaveList(History);
                ExampleInAppNotification.Show(resourceLoader.GetString("add_to_history"), 4000);
            }
            ExampleInAppNotification.Dismiss();
        }

        // открыть страницы | open pages
        private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryList.SelectedItem == null)
            {
                return;
            }



            History.Move(HistoryList.SelectedIndex, 0);
            Models.Manga.SaveList(History);
            Pages.Pages.OpenPages();
        }

        // меню | menu
        private Models.Manga selected_manga = null;
        private bool menu_is_show = false;

        private void HistoryList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (menu_is_show)
            {
                return;
            }

            ListView listView = (ListView)sender;
            Menu.ShowAt(listView, e.GetPosition(listView));
            selected_manga = ((FrameworkElement)e.OriginalSource).DataContext as Models.Manga;
           
        }
        private void HistoryList_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (menu_is_show)
            {
                return;
            }

            ListView listView = (ListView)sender;
            Menu.ShowAt(listView, e.GetPosition(listView));
            selected_manga = ((FrameworkElement)e.OriginalSource).DataContext as Models.Manga;
        }
        private void Menu_Opened(object sender, object e)
        {
            menu_is_show = true;
        }
        private void Menu_Closed(object sender, object e)
        {
            menu_is_show = false;
        }

        private void Favorit_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (selected_manga != null)
            {
                if (selected_manga.ToggleFavorit())
                {
                    ExampleInAppNotification.Show(resourceLoader.GetString("add_to_favorit"), 2000);
                }
                else
                {
                    ExampleInAppNotification.Show(resourceLoader.GetString("remove_from_favorit"), 2000);
                }
            }
        }

        private void ToChapters_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.Chapters));
        }

        private async void Refresh_ClickAsync(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (selected_manga != null)
            {
                string res = await selected_manga.Refresh();
                if (res == null)
                {
                    ExampleInAppNotification.Show(resourceLoader.GetString("chapters_updated"), 2000);
                } else
                {
                    ExampleInAppNotification.Show(res, 2000);
                }
            }
        }

        private async void InBrowser_ClickAsync(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (selected_manga != null)
            {
                Uri uri = selected_manga.GetLink();
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
                await selected_manga.Delete();
                History.Remove(selected_manga);
                ExampleInAppNotification.Show(resourceLoader.GetString("manga_remove"), 2000);
            }
            Ring.IsActive = false;
        }

        // обновление списка манги
        private void Page_GotFocus(object sender, RoutedEventArgs e)
        {
            //History.Clear();
            //Models.Manga.LoadList(ref History);
        }
    }
}