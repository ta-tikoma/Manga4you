using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
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
        SolidColorBrush color1 = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        SolidColorBrush color2 = new SolidColorBrush(Color.FromArgb(255, 115, 255, 0));
        SolidColorBrush color3 = new SolidColorBrush(Color.FromArgb(255, 179, 255, 0));

        public Sites()
        {
            this.InitializeComponent();
        }

        private Span JsonObject2Inline(JsonObject jo, string margin = "")
        {
            Span span = new Span();

            span.Inlines.Add(new Run() { Text = "{", Foreground = color1 });
            span.Inlines.Add(new LineBreak());

            foreach (string key in jo.Keys)
            {
                span.Inlines.Add(new Run() { Text = margin + "    " });
                span.Inlines.Add(new Run() { Text = key, Foreground = color2 });
                span.Inlines.Add(new Run() { Text = ":  ", Foreground = color1 });

                JsonValue jv = jo.GetNamedValue(key);
                switch (jv.ValueType)
                {
                    case JsonValueType.Null:
                        break;
                    case JsonValueType.Boolean:
                        break;
                    case JsonValueType.Number:
                        break;
                    case JsonValueType.String:
                        span.Inlines.Add(new Run() { Text = jv.GetString(), Foreground = color3 });
                        span.Inlines.Add(new LineBreak());
                        break;
                    case JsonValueType.Array:
                        break;
                    case JsonValueType.Object:
                        span.Inlines.Add(JsonObject2Inline(jv.GetObject(), margin + "    "));
                        break;
                    default:
                        break;
                }
            }

            span.Inlines.Add(new Run() { Text = margin + "}", Foreground = color1 });
            span.Inlines.Add(new LineBreak());

            return span;
        }

        private void UpdateConfigTextView()
        {
            ConfigText.Inlines.Clear();
            List<string> list = Helpers.Save.Sites.AsStrings();

            foreach (string item in list)
            {
                ConfigText.Inlines.Add(
                    JsonObject2Inline(JsonValue.Parse(item).GetObject())
                );
            }
        }

        private void ConfigText_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateConfigTextView();
        }

        private async void DownloadAsync(object sender, RoutedEventArgs e)
        {
            await Models.Config.DownloadAsync();
            UpdateConfigTextView();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values.Remove("date_last_update");
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

            string res = "[" + String.Join(",", Helpers.Save.Sites.AsStrings().ToArray()) + "]";

            if (file != null)
            {
                await Windows.Storage.FileIO.WriteTextAsync(file, res);
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
                UpdateConfigTextView();
                ExampleInAppNotification.Show("Файл конфигурации успешно загружен", 2000);
            }
        }
    }
}
