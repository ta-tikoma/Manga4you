using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Manga
{
    public sealed partial class MainPage : Page
    {
        private void ToChapters_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            VModels.Manga.InHistory inHistory = button.DataContext as VModels.Manga.InHistory;
            History.Move(History.IndexOf(inHistory), 0);
            this.Frame.Navigate(typeof(Pages.Chapters));
        }

        private async void Refresh_ClickAsync(object sender, RoutedEventArgs e)
        {
            Ring.IsActive = true;
            Button button = sender as Button;
            VModels.Manga.InHistory inHistory = button.DataContext as VModels.Manga.InHistory;
            await inHistory.Refresh();
            Ring.IsActive = false;
        }

        private async void InBrowser_ClickAsync(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            VModels.Manga.InHistory inHistory = button.DataContext as VModels.Manga.InHistory;
            Uri uri = new Uri(inHistory.Manga.Link);
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            VModels.Manga.InHistory inHistory = button.DataContext as VModels.Manga.InHistory;
            History.Remove(inHistory);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            VModels.Manga.InHistory inHistory = button.DataContext as VModels.Manga.InHistory;
            History.Move(History.IndexOf(inHistory), 0);
            this.Frame.Navigate(typeof(Pages.Pages));
        }

        private async void RefreshAll_ClickAsync(object sender, RoutedEventArgs e)
        {
            Ring.IsActive = true;
            foreach (VModels.Manga.InHistory manga in History)
            {
                await manga.Refresh();
            }
            Ring.IsActive = false;
        }
    }
}
