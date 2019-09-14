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
        ObservableCollection<VModels.Manga.InHistory> History = new ObservableCollection<VModels.Manga.InHistory>();

        public MainPage()
        {
            this.InitializeComponent();
            //System.Diagnostics.Debug.WriteLine("MasterDetailsView_ViewStateChanged:");
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            History.CollectionChanged += History_CollectionChanged;
            ApplicationView.GetForCurrentView().Title = "";
        }

        private void History_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("!History_CollectionChanged!");
            Helpers.Save.Mangas.Save(History);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        // кнопки | buttons
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.Help));
        }

        private void Thanks_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.Thanks));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.Settings));
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.Search));
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

        // проверяем ситуацию с конфигом
        private async void CheckSitesCongfig_LoadedAsync(object sender, RoutedEventArgs e)
        {
            Helpers.Request.rh = new Helpers.Request();
            await Models.Config.CheckAsync();

            Helpers.Save.Mangas.Load(ref History);

            CheckSitesCongfig.Visibility = Visibility.Collapsed;
        }
    }
}
