using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        public float width { get; set; } = 0;
        public float heigth { get; set; } = 0;

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

        public bool is_loaded = false;

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
                    }
                }
            }
        }

        private async Task LoadImageSite()
        {
            // form web
            StorageFile tempFile = await Helpers.Cache.giveMeImage(image_url, this);

            if (tempFile == null)
            {
                return;
            }

            //System.Diagnostics.Debug.WriteLine("tempFile: " + tempFile);

            _image = new BitmapImage()
            {
                DecodePixelType = DecodePixelType.Physical
            };
            try
            {
                //System.Diagnostics.Debug.WriteLine("tempFile:Path" + tempFile.Path);
                using (IRandomAccessStream fileStream = await tempFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    await _image.SetSourceAsync(fileStream);
                }
            }
            catch (Exception ex1)
            {
                try
                {
                    await tempFile.DeleteAsync();
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine("Exception2:" + ex2.Message);
                }
                System.Diagnostics.Debug.WriteLine("Exception1:" + ex1.Message);
            }

            tempFile = null;
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

            if (_image != null)
            {
                width = _image.PixelWidth;
                heigth = _image.PixelHeight;
            }

            //System.Diagnostics.Debug.WriteLine("PixelWidth:" + image.PixelWidth);
            is_loaded = true;
            RaiseProperty("width");
            RaiseProperty("heigth");
            RaiseProperty("image");

            prosent_visible = Visibility.Collapsed;
        }

        public async Task Reload()
        {
            _image = null;
            string type = Helpers.Any.GetType(image_url);
            string hash = Helpers.Any.CreateMD5(image_url);

            try
            {
                StorageFile tempFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(hash + type);
                await tempFile.DeleteAsync();
            }
            catch (Exception)
            {
            }
            await LoadImage();
        }

        // download
        public async Task<StorageFile> Download(StorageFolder folder)
        {
            string type = null;
            if (image_url.IndexOf(".jpg") != -1)
            {
                type = ".jpg";
            }

            if (image_url.IndexOf(".jpeg") != -1)
            {
                type = ".jpeg";
            }

            if (image_url.IndexOf(".gif") != -1)
            {
                type = ".gif";
            }

            if (image_url.IndexOf(".bmp") != -1)
            {
                type = ".bmp";
            }

            if (image_url.IndexOf(".png") != -1)
            {
                type = ".png";
            }

            if (type == null)
            {
                return null;
            }

            StorageFile sampleFile = await folder.CreateFileAsync(number + type, CreationCollisionOption.ReplaceExisting);
            Byte[] bytes = await (new HttpClient()).GetByteArrayAsync(image_url);
            IBuffer buffer = bytes.AsBuffer();
            await FileIO.WriteBufferAsync(sampleFile, buffer);
            return sampleFile;
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
