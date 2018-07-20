﻿using Microsoft.Toolkit.Uwp.UI.Controls;
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
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
        } 

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        // cache
        private void clearCache_Click(object sender, RoutedEventArgs e)
        {
            Ring.IsActive = true;
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            try
            {
                StorageFolder localDirectory = ApplicationData.Current.LocalFolder;
                string[] tmpCacheDirectories = Directory.GetDirectories(localDirectory.Path + "\\..\\ac\\inetcache");
                foreach (string dir in tmpCacheDirectories)
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        if (File.GetLastAccessTime(file) < DateTime.Now.AddMinutes(-1))
                            try
                            {
                                File.Delete(file);
                                System.Diagnostics.Debug.WriteLine("Deleted:" + file);
                            }
                            catch (Exception) { }
                    }
                }

                ExampleInAppNotification.Show(resourceLoader.GetString("cache_clear"), 2000);
            }
            catch (Exception ex)
            {
                ExampleInAppNotification.Show(resourceLoader.GetString("cache_clear_error"), 2000);
                System.Diagnostics.Debug.WriteLine("ERROR CLEANING CACHE:" + ex.Message);
            }
            clearCacheComplite.Visibility = Visibility.Visible;
            Ring.IsActive = false;
        }

        // sites
        private void SitesWithManga_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Sites));
        }
    }
}