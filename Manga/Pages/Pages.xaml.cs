using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.Profile;
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
    public sealed partial class Pages : Page
    {
        Models.Manga Manga = null;

        public Pages()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("manga_0"))
            {
                Manga = new Models.Manga(JsonValue.Parse(localSettings.Values["manga_0"].ToString()).GetObject(), 0);
            }
            else
            {
                Window.Current.Close();
                return;
            }

            this.InitializeComponent();
            HideStatusBarAsync();
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

        // к следующей главе
        bool last_page_check = false;
        private void MangaPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MangaPages.SelectedItem != null)
            {
                ApplicationView.GetForCurrentView().Title = (MangaPages.SelectedIndex + 1) + " / " + MangaPages.Items.Count();
                if (MangaPages.SelectedIndex + 1 == MangaPages.Items.Count())
                {
                    if (MangaPages.SelectedIndex == 0)
                    {
                        ApplicationView.GetForCurrentView().Title = "";
                    }
                }

                if (Manga.IsArchive())
                {
                    return;
                }

                if (MangaPages.SelectedIndex + 1 == MangaPages.Items.Count)
                {
                    last_page_check = true;
                }
                else if ((MangaPages.SelectedIndex == 0) && last_page_check)
                {
                    last_page_check = false;

                    if (Manga.current_chapter != 0)
                    {
                        Manga.current_chapter -= 1;
                    }
                    else
                    {
                        Window.Current.Close();
                    }
                }
                else
                {
                    last_page_check = false;
                }
            }
        }

        // Zoom
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            Manga.zoom = scrollViewer.ZoomFactor;
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ChangeView(null, null, Manga.zoom);
        }

        // открыть страницы
        public static async Task OpenPages(Page ths)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                ths.Frame.Navigate(typeof(Pages));
            }
            else
            {
                CoreApplicationView newView = CoreApplication.CreateNewView();
                int newViewId = 0;
                await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Frame frame = new Frame();
                    frame.Navigate(typeof(Pages), null);
                    Window.Current.Content = frame;
                    Window.Current.Activate();

                    newViewId = ApplicationView.GetForCurrentView().Id;
                });
                bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
            }
        }

        // переводчик
        private void TranslateClose_Click(object sender, RoutedEventArgs e)
        {
            TanslatePanel.Visibility = Visibility.Collapsed;
        }

        private void PivotGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            TranslateInput.Text = "";
            TranslateOutput.Text = "";
            TanslatePanel.Visibility = Visibility.Visible;
        }

        private void PivotGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            TanslatePanel.Visibility = Visibility.Visible;
        }

        private async void TranslateInput_KeyUpAsync(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string res = await Helpers.Request.rh.Get(
                    "https://translate.yandex.net/api/v1.5/tr.json/translate?lang=en-ru&text=" + TranslateInput.Text + "&key=trnsl.1.1.20171024T153244Z.67d79b01c3416714.c7af5dc647523bcc3b99ea2555310ad8e9954618"
                    );

                JsonObject root = JsonValue.Parse(res).GetObject();
                System.Diagnostics.Debug.WriteLine("root:" + root);
                if (root == null)
                {
                    return;
                }

                if (!root.ContainsKey("text"))
                {
                    return;
                }

                string variants = "";
                foreach (JsonValue variant in root.GetNamedArray("text"))
                {
                    if (variants.Length == 0)
                    {
                        variants += variant.GetString();
                    }
                    else
                    {
                        variants += "," + variant.GetString();
                    }
                }

                TranslateOutput.Text = variants;
            }
        }

        private void TranslateInput_GotFocus(object sender, RoutedEventArgs e)
        {
            MangaPages.Margin = new Thickness(0, 260, 0, 0);
        }

        private void TranslateInput_LostFocus(object sender, RoutedEventArgs e)
        {
            MangaPages.Margin = new Thickness(0, 0, 0, 0);
        }
    }
}
