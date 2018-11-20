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

            // Next chapter
            if (page.number == Models.Page.NEXT_CHAPTER)
            {
                if (Manga.IsArchive())
                {
                    ClosePages();
                }
                else
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
                return;
            }

            // Scroll to 0 0 then page open
            if (page.is_loaded)
            {
                SetZoom();
            } else
            {
                page.PropertyChanged += Page_PropertyChanged;
            }
            //ApplicationView.GetForCurrentView().Title = (MangaPages.SelectedIndex + 1) + " / " + Manga.pages_count;
        }

        private void Page_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Models.Page page = sender as Models.Page;
            if (page.is_loaded)
            {
               SetZoom();
            }
        }

        private void SetZoom()
        {
            foreach (ScrollViewer scrollViewer in scrollViewers)
            {
                //if (page.number == scrollViewer.Tag.ToString())
                {
                    if (Manga.auto_zoom)
                    {
                        Image image = scrollViewer.Content as Image;
                        //System.Diagnostics.Debug.WriteLine("MangaPages_SelectionChanged:");
                        //System.Diagnostics.Debug.WriteLine("image.ActualWidth:" + image.ActualWidth);
                        //System.Diagnostics.Debug.WriteLine("MangaPages.ActualWidth:" + MangaPages.ActualWidth);

                        if (image.ActualWidth != 0)
                        {
                            scrollViewer.ChangeView(0, 0, (float)(MangaPages.ActualWidth / image.ActualWidth));
                            continue;
                            //return;
                        }
                    }
                    scrollViewer.ChangeView(0, 0, Manga.zoom);
                    //return;
                }
            }
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
            Manga.zoom = scrollViewer.ZoomFactor;
            if (!Manga.auto_zoom)
            {
                foreach (ScrollViewer sv in scrollViewers)
                {
                    if (sv == scrollViewer)
                    {
                        continue;
                    }

                    sv.ChangeView(null, null, Manga.zoom);
                }
            }
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
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
        }

        public static void ClosePages()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;

            rootFrame.GoBack();
        }

        // меню | menu
        private bool menu_is_show = false;
        private Grid selected_grid = null;

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Menu_Show(
                sender as Grid,
                e.GetPosition(sender as UIElement)
            );
        }
        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Menu_Show(
                sender as Grid,
                e.GetPosition(sender as UIElement)
            );
        }
        private void Menu_Show(Grid grid, Point point)
        {
            if (menu_is_show)
            {
                return;
            }
            selected_grid = grid;
            PageMenu.ShowAt(grid, point);
        }
        private void Menu_Opened(object sender, object e)
        {
            menu_is_show = true;
        }
        private void Menu_Closed(object sender, object e)
        {
            menu_is_show = false;
        }

        private void AutoZoom_Loaded(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (Manga.auto_zoom)
            {
                AutoZoom.Text = "✔ " + resourceLoader.GetString("auto_zoom");
            }
            else
            {
                AutoZoom.Text = resourceLoader.GetString("auto_zoom");
            }
        }

        private void AutoZoomMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Manga.auto_zoom = !Manga.auto_zoom;
            AutoZoom_Loaded(sender, e);
        }

        private void InWidthMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = selected_grid.Children.ElementAt(1) as ScrollViewer;
            Image image = scrollViewer.Content as Image;
            if (image.ActualWidth == 0)
            {
                return;
            }
            //System.Diagnostics.Debug.WriteLine("InWidthMenuFlyoutItem_Click:");
            //System.Diagnostics.Debug.WriteLine("image.ActualWidth:" + image.ActualWidth);
            //System.Diagnostics.Debug.WriteLine("MangaPages.ActualWidth:" + MangaPages.ActualWidth);
            scrollViewer.ChangeView(null, null, (float) (MangaPages.ActualWidth / image.ActualWidth));
        }

        private void RefreshMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            (MangaPages.SelectedItem as Models.Page).Reload();
        }

        private async void SaveMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (await (MangaPages.SelectedItem as Models.Page).Save())
            {
                ExampleInAppNotification.Show(resourceLoader.GetString("image_save"), 4000);
            } else
            {
                ExampleInAppNotification.Show(resourceLoader.GetString("image_not_save"), 4000);
            }
        }

        // переводчик | translate
        private void TranslateClose_Click(object sender, RoutedEventArgs e)
        {
            TanslatePanel.Visibility = Visibility.Collapsed;
        }

        private async void TranslateInput_KeyUpAsync(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string res = await Helpers.Request.rh.Get(
                    "https://translate.yandex.net/api/v1.5/tr.json/translate?lang=" +
                        App.Settings.translate_code +
                        "&text=" + 
                        TranslateInput.Text + 
                        "&key=" +
                        App.Settings.translate_key
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
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MangaPages.Margin = new Thickness(0, 260, 0, 0);
            }
        }

        private void TranslateInput_LostFocus(object sender, RoutedEventArgs e)
        {
            MangaPages.Margin = new Thickness(0, 0, 0, 0);
        }

        private void MangaPages_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (App.Settings.translate_key.Length == 0)
            {
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                ExampleInAppNotification.Show(resourceLoader.GetString("enter_translate_key"), 4000);
                return;
            }
            TranslateInput.Text = "";
            TranslateOutput.Text = "";
            TanslatePanel.Visibility = Visibility.Visible;
        }
    }
}
