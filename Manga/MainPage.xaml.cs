using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
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
        ObservableCollection<VModels.Manga.InHistory> History = new ObservableCollection<VModels.Manga.InHistory>();

        public MainPage()
        {
            this.InitializeComponent();
            //System.Diagnostics.Debug.WriteLine("MasterDetailsView_ViewStateChanged:");
            HideStatusBarAsync();
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            History.CollectionChanged += History_CollectionChanged;
            ApplicationView.GetForCurrentView().Title = "";
        }

        private void History_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("!History_CollectionChanged!");
            Helpers.Save.Mangas.Save(History);
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

        // кнопки | buttons
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.Settings));
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.Help));
        }

        private void Thanks_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.Thanks));
        }

        // проверяем ситуацию с конфигом
        private async void CheckSitesCongfig_LoadedAsync(object sender, RoutedEventArgs e)
        {
            Helpers.Request.rh = new Helpers.Request();
            await Models.Config.CheckAsync();

            Helpers.Save.Mangas.Load(ref History);
            Helpers.Save.Sites.Load(ref Sites);

            CheckSitesCongfig.Visibility = Visibility.Collapsed;
        }
    }
}
