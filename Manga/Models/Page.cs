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
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Manga.Models
{
    class Page : INotifyPropertyChanged
    {
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
                System.Diagnostics.Debug.WriteLine("image:get");
                if (_image == null)
                {
                    LoadImage();
                }
                return _image;
            }
        }

        private async Task LoadImage()
        {
            if (_image == null)
            {
                prosent_visible = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine("image:" + image_url);

                if (image_url.Substring(0, 4) != "http")
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
                else
                {
                    // form web
                    cts = new CancellationTokenSource();
                    byte[] byteArray = null;
                    byteArray = await Helpers.Request.GetFileStatic(image_url, this);
                    cts.Dispose();
                    cts = null;

                    System.Diagnostics.Debug.WriteLine("byteArray:" + byteArray);
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
               
                prosent_visible = Visibility.Collapsed;
            }
        }

        /*private async Task LoadImage()
        {
            if (_image == null)
            {
                prosent_visible = Visibility.Visible;
                cts = new CancellationTokenSource();
                byte[] byteArray = null;
                System.Diagnostics.Debug.WriteLine("image:" + image_url);
                if (image_url.Substring(0, 4) != "http")
                {
                    // from archive
                    string[] parts = image_url.Split('\\');
                    System.Diagnostics.Debug.WriteLine("folder:name" + parts[0]);
                    StorageFolder folder = (StorageFolder) await ApplicationData.Current.LocalFolder.TryGetItemAsync(parts[0]);
                    System.Diagnostics.Debug.WriteLine("folder:" + folder);
                    if (folder != null)
                    {
                        System.Diagnostics.Debug.WriteLine("folder:path" + folder.Path);
                        StorageFile file = (StorageFile) await folder.TryGetItemAsync(parts[1]);
                        System.Diagnostics.Debug.WriteLine("file:name" + parts[1]);
                        System.Diagnostics.Debug.WriteLine("file:" + file);
                        if (file != null)
                        {
                            System.Diagnostics.Debug.WriteLine("file:path" + file.Path);
                            var stream = await file.OpenStreamForReadAsync();
                            byteArray = new byte[(int)stream.Length];
                            stream.Read(byteArray, 0, (int)stream.Length);
                        }
                    }
                }
                else
                {
                    // form web
                    byteArray = await Helpers.Request.GetFileStatic(image_url, this);
                }
                cts.Dispose();
                cts = null;

                System.Diagnostics.Debug.WriteLine("byteArray:" + byteArray);
                if (byteArray != null)
                {
                    _image = new BitmapImage();
                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        stream.WriteAsync(byteArray.AsBuffer()).GetResults();
                        stream.Seek(0);
                        await _image.SetSourceAsync(stream);
    }
                    RaiseProperty("image");
                    byteArray = null;
                }
                prosent_visible = Visibility.Collapsed;
            }
        }*/
        /*
        public void Cancel()
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }
        */
    }
}
