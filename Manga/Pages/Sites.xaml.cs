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
        public Sites()
        {
            this.InitializeComponent();
        }

        private void ConfigText_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigText.Text = Models.Config.AsString();
        }

        private async void DownloadAsync(object sender, RoutedEventArgs e)
        {
            await Models.Config.DownloadAsync();
            ConfigText.Text = Models.Config.AsString();
            ExampleInAppNotification.Show("Файл конфигурации успешно загружен", 2000);
        }

        private async void OpenLinkInBrowser(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(
                new Uri(Models.Config.CONFIG_LINK)
                );
        }

        private async void Save(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Json", new List<string>() { ".json" });
            savePicker.SuggestedFileName = "sites";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                await Windows.Storage.FileIO.WriteTextAsync(file, Models.Config.AsString());
                ExampleInAppNotification.Show("Файл конфигурации успешно сохранен", 2000);
            }
        }

        private async void Load(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".json");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string text = await Windows.Storage.FileIO.ReadTextAsync(file);
                Models.Config.Load(text);
                ConfigText.Text = Models.Config.AsString();
                ExampleInAppNotification.Show("Файл конфигурации успешно загружен", 2000);
            }
        }
    }
}
