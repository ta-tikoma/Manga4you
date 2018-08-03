using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class AddSite : Page
    {
        string name { get; set; } = "";
        string search_link { get; set; } = "";
        string search_post { get; set; } = "";
        string search_regexp { get; set; } = "";
        string chapters_link { get; set; } = "";
        string chapters_regexp { get; set; } = "";
        string pages_link { get; set; } = "";
        string pages_regexp { get; set; } = "";

        public AddSite()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainPivot.SelectedIndex == 0)
            {
                MainPivot.SelectedIndex = (MainPivot.Items.Count() - 1);
            }
            else
            {
                MainPivot.SelectedIndex--;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainPivot.SelectedIndex == (MainPivot.Items.Count() - 1))
            {
                MainPivot.SelectedIndex = 0;
            } else
            {
                MainPivot.SelectedIndex++;
            }
        }

        

        private async void ComplitButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (
                (name.Length > 0) &&
                (search_link.Length > 0) &&
                (search_regexp.Length > 0) &&
                (chapters_link.Length > 0) &&
                (chapters_regexp.Length > 0) &&
                (pages_link.Length > 0) &&
                (pages_regexp.Length > 0)
                )
            {
                ObservableCollection<Models.Site> site_list = new ObservableCollection<Models.Site>();
                Models.Site.LoadList(ref site_list);
                Models.Site site = new Models.Site()
                {
                    name = name,
                    search_link = search_link,
                    search_post = search_post,
                    search_regexp = search_regexp,
                    chapters_link = chapters_link,
                    chapters_regexp = chapters_regexp,
                    pages_link = pages_link,
                    pages_regexp = pages_regexp
                };
                site_list.Add(site);
                Models.Site.SaveList(site_list);

                if (ShareToAll.IsOn)
                {
                    await Helpers.Any.ShareBy(site);
                }

                this.Frame.Navigate(typeof(Sites));
            }
            else
            {
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                ExampleInAppNotification.Show(resourceLoader.GetString("all_fields_must_be_fill"), 2000);
            }
        }
    }
}
