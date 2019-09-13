using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Manga
{
    public sealed partial class MainPage : Page
    {
        // поиск | search
        private async void Search_KeyUpAsync(object sender, KeyRoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (SiteSelect.SelectedItem == null)
                {
                    ExampleInAppNotification.Show(resourceLoader.GetString("select_site"), 2000);
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
                            joSearch.GetNamedString("link").Replace("#word#", Search.Text)
                            );
                    }
                    else
                    {
                        res = await Helpers.Request.rh.Post(
                            joSearch.GetNamedString("link"),
                            joSearch.GetNamedString("post").Replace("#word#", Search.Text)
                            );
                    }
                    if (res == null)
                    {
                        ExampleInAppNotification.Show(resourceLoader.GetString("request_failed"), 4000);
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
    }
}
