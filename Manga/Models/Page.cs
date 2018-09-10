using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Manga.Models
{
    class Page : INotifyPropertyChanged
    {
        public const string NEXT_CHAPTER = "loading...";

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public CancellationTokenSource cts = null;

        public int _prosent { get; set; } = 0;
        public int prosent
        {
            get { return _prosent; }
            set { if ((value >= 0) && (value <= 100)) { _prosent = value; } RaiseProperty("prosent"); }
        }

        public Visibility _prosent_visible { get; set; } = Visibility.Collapsed;
        public Visibility prosent_visible
        {
            get { return _prosent_visible; }
            set { _prosent_visible = value; RaiseProperty("prosent_visible"); }
        }

        public string number { get; set; } = "1";
        public string image_url { get; set; } = "";

        private BitmapImage _image = null;
        public BitmapImage image
        {
            get {
                if (_image == null)
                {
                    LoadImage();
                }
                return _image;
            }
        }

        // load
        private async Task LoadImageArchive()
        {
            // from archive
            string[] parts = image_url.Split('\\');
            StorageFolder folder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(parts[0]);
            if (folder != null)
            {
                StorageFile file = (StorageFile)await folder.TryGetItemAsync(parts[1]);
                if (file != null)
                {
                    using (var randomAccessStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        _image = new BitmapImage();
                        await _image.SetSourceAsync(randomAccessStream);
                        RaiseProperty("image");
                    }
                }
            }
        }

        private async Task LoadImageSite()
        {
            // form web
            cts = new CancellationTokenSource();
            byte[] byteArray = null;
            byteArray = await Helpers.Request.GetFileStatic(image_url, this);
            cts.Dispose();
            cts = null;

            if (byteArray != null)
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    stream.WriteAsync(byteArray.AsBuffer()).GetResults();
                    stream.Seek(0);

                    _image = new BitmapImage();
                    await _image.SetSourceAsync(stream);
                }
                byteArray = null;
                RaiseProperty("image");
            }
        }

        private async Task LoadImage()
        {
            if (prosent_visible == Visibility.Visible)
            {
                return;
            }

            prosent_visible = Visibility.Visible;

            if (image_url.Length == 0)
            {
                _image = new BitmapImage();
            }
            else if (image_url.Substring(0, 4) != "http")
            {
                await LoadImageArchive();
            }
            else
            {
                await LoadImageSite();
            }
               
            prosent_visible = Visibility.Collapsed;
        }

        public async Task Reload()
        {
            _image = null;
            await LoadImage();
        }

        // save
        private async Task SaveImageArchive(StorageFile savefile)
        {
            string[] parts = image_url.Split('\\');
            StorageFolder folder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(parts[0]);
            if (folder != null)
            {
                StorageFile file = (StorageFile)await folder.TryGetItemAsync(parts[1]);
                if (file != null)
                {
                    await file.CopyAndReplaceAsync(savefile);
                }
            }
        }

        private async Task SaveImageSite(StorageFile savefile)
        {
            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(new Uri(image_url), savefile);
            await download.StartAsync();
            downloader = null;
            download = null;
        }

        public async Task<bool> Save()
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("Image", new List<string>() { ".jpg" });
            savePicker.SuggestedFileName = "Manga_" + number + "_" + DateTime.Now.ToString("yyyyMMddhhmmss");
            StorageFile savefile = await savePicker.PickSaveFileAsync();
            if (savefile == null)
                return false;

            if (image_url.Substring(0, 4) != "http")
            {
                await SaveImageArchive(savefile);
            }
            else
            {
                await SaveImageSite(savefile);
            }

            return true;
        }
    }
}
