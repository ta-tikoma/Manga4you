using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class Sites : Page
    {
        ObservableCollection<Models.Site> site_list = new ObservableCollection<Models.Site>();

        public Sites()
        {
            Models.Site.LoadList(ref site_list);
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += Settings_BackRequested;
            site_list.CollectionChanged += Sites_CollectionChanged;
        }

        private void Sites_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Models.Site.SaveList(site_list);
        }

        private void Settings_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (SitesList.ViewState == MasterDetailsViewState.Master)
            {
                Models.Site.SaveList(site_list);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddSite));

            /*
            Models.Site site = new Models.Site();
            site_list.Add(site);
            SitesList.SelectedItem = site;*/
        }

        private async void Default_ClickAsync(object sender, RoutedEventArgs e)
        {
            await Models.Site.SetDefaultSites();
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            site_list.Clear();
            Models.Site.LoadList(ref site_list);
            ExampleInAppNotification.Show(resourceLoader.GetString("config_loaded"), 2000);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Models.Site.DeleteList(ref site_list);
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ExampleInAppNotification.Show(resourceLoader.GetString("all_site_delete"), 2000);
        }

        private async void Link_ClickAsync(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/ta-tikoma/Manga4you"));
        }

        // Меню
        private Models.Site selected_site = null;
        private bool menu_is_show = false;
        private void SitesList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (menu_is_show)
            {
                return;
            }

            MasterDetailsView listView = (MasterDetailsView)sender;
            Menu.ShowAt(listView, e.GetPosition(listView));
            selected_site = ((FrameworkElement)e.OriginalSource).DataContext as Models.Site;
        }
        private void SitesList_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (menu_is_show)
            {
                return;
            }

            MasterDetailsView listView = (MasterDetailsView)sender;
            Menu.ShowAt(listView, e.GetPosition(listView));
            selected_site = ((FrameworkElement)e.OriginalSource).DataContext as Models.Site;
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (selected_site != null)
            {
                if (selected_site.Delete())
                {
                    ExampleInAppNotification.Show(resourceLoader.GetString("site_deleted"), 2000);
                }
                else
                {
                    ExampleInAppNotification.Show(resourceLoader.GetString("site_not_found"), 2000);
                }
                site_list.Remove(selected_site);
            }
            else
            {
                ExampleInAppNotification.Show(resourceLoader.GetString("site_not_found"), 2000);
            }
        }
        private async void Share_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (selected_site != null)
            {
                await Helpers.Any.ShareBy(selected_site);
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
    }
}
