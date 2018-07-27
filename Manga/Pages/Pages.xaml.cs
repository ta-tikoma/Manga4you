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
        private void MangaPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MangaPages.SelectedItem == null)
            {
                return;
            }

            Models.Page page = MangaPages.SelectedItem as Models.Page;
            if (page.number == Models.Page.NEXT_CHAPTER)
            {
                if (Manga.IsArchive())
                {
                    ClosePages();
                } else
                {
                    if ((Manga.current_chapter + 1).ToString() != Manga.chapters_count)
                    {
                        Manga.current_chapter += 1;
                    }
                    else
                    {
                        ClosePages();
                    }
                }
            }

            //ApplicationView.GetForCurrentView().Title = (MangaPages.SelectedIndex + 1) + " / " + Manga.pages_count;
        }

        // Zoom
        List<ScrollViewer> scrollViewers = new List<ScrollViewer>();

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (MangaPages.SelectedItem == null)
            {
                return;
            }

            Models.Page page = MangaPages.SelectedItem as Models.Page;

            ScrollViewer scrollViewer = sender as ScrollViewer;

            if (page.number.ToString() != scrollViewer.Tag.ToString())
            {
                return;
            }

            // it ScrollViewer of current page

            if (Manga.zoom == scrollViewer.ZoomFactor)
            {
                return;
            }

            Manga.zoom = scrollViewer.ZoomFactor;

            foreach (ScrollViewer sv in scrollViewers)
            {
                if (sv == scrollViewer)
                {
                    continue;
                }

                sv.ChangeView(null, null, Manga.zoom);
            }
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ChangeView(null, null, Manga.zoom);
            scrollViewers.Add(scrollViewer);
        }

        // открыть страницы
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        public static async Task OpenPages(Page ths)
        {
            ths.Frame.Navigate(typeof(Pages));
            return;
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

        public static void ClosePages()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;

            rootFrame.GoBack();
        }

        // переводчик
        private void TranslateClose_Click(object sender, RoutedEventArgs e)
        {
            TanslatePanel.Visibility = Visibility.Collapsed;
        }

        private void FlipView_Holding(object sender, HoldingRoutedEventArgs e)
        {
            TranslateInput.Text = "";
            TranslateOutput.Text = "";
            TanslatePanel.Visibility = Visibility.Visible;
        }

        private void FlipView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlipView_Holding(sender, new HoldingRoutedEventArgs());
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
