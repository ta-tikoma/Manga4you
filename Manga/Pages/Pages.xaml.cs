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
        VModels.Manga.OnPagesPage MangaOnPagesPage = new VModels.Manga.OnPagesPage();

        public Pages()
        {
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

        bool savePage = false;

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
                if (MangaOnPagesPage.Manga.CurrentChapter + 1 != MangaOnPagesPage.Manga.ChaptersCount)
                {
                    MangaOnPagesPage.NextChapter();
                    Helpers.Cache.checkAndFixSize();
                }
                else
                {
                    ClosePages();
                    return;
                }
            }

            // Scroll to 0 0 then page open
            if (page.is_loaded)
            {
                SetZoomAll();
            } else
            {
                //System.Diagnostics.Debug.WriteLine("MangaPages_SelectionChanged:" + page.number);
                page.PropertyChanged += Page_PropertyChanged;
            }
            ApplicationView.GetForCurrentView().Title = (MangaPages.SelectedIndex + 1) + " / " + MangaOnPagesPage.Manga.PagesCount;
        }

        private void Page_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "image")
            {
                return;
            }

            Models.Page page = sender as Models.Page;



            //System.Diagnostics.Debug.WriteLine("Page_PropertyChanged:e.PropertyName:" + e.PropertyName);
            //System.Diagnostics.Debug.WriteLine("Page_PropertyChanged:page.is_loaded:" + page.is_loaded);
            if (page.is_loaded)
            {
                SetZoomAll();
            }
        }

        private void SetZoomAll()
        {
            foreach (ScrollViewer scrollViewer in scrollViewers)
            {
                if (scrollViewer.DataContext == MangaPages.SelectedItem)
                {
                    SetZoomOne(scrollViewer);
                }
            }
        }

        private void SetZoomOne(ScrollViewer scrollViewer)
        {
            if (MangaOnPagesPage.Manga.AutoZoom)
            {
                Models.Page page = scrollViewer.DataContext as Models.Page;

                //System.Diagnostics.Debug.WriteLine("----------------------------------");
                //System.Diagnostics.Debug.WriteLine("MangaPages_SelectionChanged:" + scrollViewer.Tag);
                //System.Diagnostics.Debug.WriteLine("page.width:" + page.width);
                //System.Diagnostics.Debug.WriteLine("MangaPages.ActualWidth:" + MangaPages.ActualWidth);

                if (page.width != 0)
                {
                    scrollViewer.ChangeView(0, 0, (float)(MangaPages.ActualWidth / (page.width + 2)));
                    return;
                }
            }
            scrollViewer.ChangeView(0, 0, MangaOnPagesPage.Manga.Zoom);
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
            MangaOnPagesPage.SetZoom(scrollViewer.ZoomFactor);
            if (!MangaOnPagesPage.Manga.AutoZoom)
            {
                foreach (ScrollViewer sv in scrollViewers)
                {
                    if (sv == scrollViewer)
                    {
                        continue;
                    }

                    sv.ChangeView(null, null, MangaOnPagesPage.Manga.Zoom);
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
            if (MangaOnPagesPage.Manga.AutoZoom)
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
            MangaOnPagesPage.ToggleAutoZoom();
            AutoZoom_Loaded(sender, e);
        }

        private void InWidthMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = selected_grid.Children.ElementAt(1) as ScrollViewer;
            SetZoomOne(scrollViewer);
            /*
            Image image = scrollViewer.Content as Image;
            if (image.ActualWidth == 0)
            {
                return;
            }
            //System.Diagnostics.Debug.WriteLine("InWidthMenuFlyoutItem_Click:");
            //System.Diagnostics.Debug.WriteLine("image.ActualWidth:" + image.ActualWidth);
            //System.Diagnostics.Debug.WriteLine("MangaPages.ActualWidth:" + MangaPages.ActualWidth);
            scrollViewer.ChangeView(null, null, (float) (MangaPages.ActualWidth / image.ActualWidth));
            */
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

        private void CopyPageLinkMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText((MangaPages.SelectedItem as Models.Page).image_url);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ExampleInAppNotification.Show(resourceLoader.GetString("LinkIsCopyied"), 4000);
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
