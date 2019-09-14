using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    public sealed partial class Search : Page
    {
        ObservableCollection<Models.Site> Sites = new ObservableCollection<Models.Site>();
        ObservableCollection<VModels.Manga.InHistory> History = new ObservableCollection<VModels.Manga.InHistory>();

        public Search()
        {
            this.InitializeComponent();
            History.CollectionChanged += History_CollectionChanged;
            Helpers.Save.Mangas.Load(ref History);
            Helpers.Save.Sites.Load(ref Sites);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void History_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("!History_CollectionChanged!");
            Helpers.Save.Mangas.Save(History);
        }

        // поиск | search
        private async void Search_KeyUpAsync(object sender, KeyRoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (SiteSelect.SelectedItem == null)
                {
                    //ExampleInAppNotification.Show(resourceLoader.GetString("select_site"), 2000);
                }
                else
                {
                    Models.Site site = SiteSelect.SelectedItem as Models.Site;
                    Ring.IsActive = true;
                    string res = null;

                    JsonObject joSearch = site.GetJsonObject(Models.Site.JO_TYPE_SEARCH);

                    if (joSearch.GetNamedString("post").Trim().Length == 0)
                    {
                        res = await Helpers.Request.rh.Get(
                            joSearch.GetNamedString("link").Replace("#word#", SearchInput.Text)
                            );
                    }
                    else
                    {
                        res = await Helpers.Request.rh.Post(
                            joSearch.GetNamedString("link"),
                            joSearch.GetNamedString("post").Replace("#word#", SearchInput.Text)
                            );
                    }
                    if (res == null)
                    {
                        // ExampleInAppNotification.Show(resourceLoader.GetString("request_failed"), 4000);
                        Ring.IsActive = false;
                        return;
                    }

                    ObservableCollection<VModels.Manga.InSearch> MangaList = new ObservableCollection<VModels.Manga.InSearch>();

                    try
                    {
                        List<string> links = Helpers.Regular.GetValuesByJO(
                            site.GetJsonObject(Models.Site.JO_TYPE_MANGA, Models.Site.JO_PATH_LINK),
                            res
                        );

                        List<string> names = Helpers.Regular.GetValuesByJO(
                            site.GetJsonObject(Models.Site.JO_TYPE_MANGA, Models.Site.JO_PATH_NAME),
                            res
                        );

                        for (int i = 0; i < links.Count; i++)
                        {
                            MangaList.Add(
                                new VModels.Manga.InSearch(
                                    new Models.Manga(names[i], links[i], site.hash),
                                    History
                                )
                            );
                        }
                    }
                    catch (Exception exception)
                    {
                        //ExampleInAppNotification.Show(resourceLoader.GetString("mask_error") + ": " + exception.Message, 4000);
                        Ring.IsActive = false;
                        return;
                    }


                    if (MangaList.Count() == 0)
                    {
                        //ExampleInAppNotification.Show(resourceLoader.GetString("nothing_found"), 4000);
                        Ring.IsActive = false;
                        return;
                    }

                    SearchResultList.ItemsSource = MangaList;

                    //SearchResult.Visibility = Visibility.Visible;
                    //DeleteSearch.Focus(FocusState.Programmatic);
                    Ring.IsActive = false;
                }
            }
        }

        // добавить | add
        private void ToggleManga_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            VModels.Manga.InSearch searchManga = (sender as Button).DataContext as VModels.Manga.InSearch;
            switch (searchManga.Toggle(ref History))
            {
                case VModels.Manga.SearchMangaType.NOW_ADDED:
                    //ExampleInAppNotification.Show(resourceLoader.GetString("add_to_history"), 4000);
                    break;
                case VModels.Manga.SearchMangaType.NOT_ADDED:
                    //ExampleInAppNotification.Show(resourceLoader.GetString("remove_from_history"), 4000);
                    break;
            }
        }
    }
}
