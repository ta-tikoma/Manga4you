using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Manga.Helpers
{
    class Request
    {
        HttpClient client;
        public static Request rh = null;
        public static string referer = null;

        public Request()
        {
            client = new HttpClient();
        }

        public async Task<string> Get(string url)
        {
            //System.Diagnostics.Debug.WriteLine("url:" + url);
            client.DefaultRequestHeaders.Remove("Referer");
            try
            {
                HttpResponseMessage response = await client.GetAsync(new Uri(url));
                if (response.IsSuccessStatusCode)
                {
                    referer = url;
                    // var res = await response.Content.ReadAsStringAsync();
                    //System.Diagnostics.Debug.WriteLine("res:" + res);
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> Post(string url, string arg)
        {
            //System.Diagnostics.Debug.WriteLine("url:" + url);
            //System.Diagnostics.Debug.WriteLine("arg:" + arg);
            client.DefaultRequestHeaders.Remove("Referer");

            try
            {
                HttpMultipartFormDataContent content = new HttpMultipartFormDataContent();
                foreach (string part in arg.Split('&'))
                {
                    string[] parts = part.Split('=');
                    if (parts.Count() == 2)
                    {
                        content.Add(new HttpStringContent(parts[1]), parts[0]);
                    }
                }

                HttpResponseMessage response = await this.client.PostAsync(new Uri(url), content);
                if (response.IsSuccessStatusCode)
                {
                    referer = url;
                    //var res = await response.Content.ReadAsStringAsync();
                    //System.Diagnostics.Debug.WriteLine("res:" + res);
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task DownloadFile(string url, StorageFile file, Models.Page page)
        {
            Progress<HttpProgress> progressCallback = new Progress<HttpProgress>((HttpProgress obj) => {
                if (obj.TotalBytesToReceive == null)
                {
                    return;
                }
                int value = (int)(obj.BytesReceived / (obj.TotalBytesToReceive / 100));
                page.prosent = value;
            });
            var tokenSource = new CancellationTokenSource();
            HttpResponseMessage response = await client.GetAsync(new Uri(url)).AsTask(tokenSource.Token, progressCallback);
            tokenSource.Dispose();

            IBuffer buffer = await response.Content.ReadAsBufferAsync();
            byte[] rawBytes = new byte[buffer.Length];
            using (var reader = DataReader.FromBuffer(buffer))
            {
                reader.ReadBytes(rawBytes);
            }

            using (Stream stream = await file.OpenStreamForWriteAsync())
            {
                stream.Write(rawBytes, 0, rawBytes.Length);
            }
            /*
            await Windows.Storage.FileIO.WriteTextAsync(
                file,
                await response.Content.ReadAsStringAsync()
                );
            */
        }

        public async Task<byte[]> GetFile(string url, CancellationToken ct)//, RadialProgressBar progressBar)
        {
            try
            {
                Progress<HttpProgress> progressCallback = new Progress<HttpProgress>((HttpProgress obj) => {
                    if (obj.TotalBytesToReceive == null)
                    {
                        return;
                    }
                    double value = (double) (obj.BytesReceived / (obj.TotalBytesToReceive / 100));
                    /*if ((value <= progressBar.Maximum) && (value >= progressBar.Minimum))
                    {
                        progressBar.Value = value;
                    }*/
                });

                client.DefaultRequestHeaders["Referer"] = referer;
                HttpResponseMessage response = await client.GetAsync(new Uri(url)).AsTask(ct, progressCallback);

                if (response.IsSuccessStatusCode)
                {
                    IBuffer buffer = await response.Content.ReadAsBufferAsync();

                    byte[] rawBytes = new byte[buffer.Length];
                    using (var reader = DataReader.FromBuffer(buffer))
                    {
                        reader.ReadBytes(rawBytes);
                    }

                    return rawBytes;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("IsSuccessStatusCode:" + response.StatusCode);
                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("OperationCanceledException:");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ex:" + ex.Message);
                return null;
            }
        }
    }
}
