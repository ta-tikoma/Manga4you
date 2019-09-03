using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Manga.Helpers
{
    class Cache
    {
        public static async Task<StorageFile> giveMeImage(string link, Models.Page page)
        {
            StorageFile tempFile = null;
            string type = Helpers.Any.GetType(link);
            string hash = Helpers.Any.CreateMD5(link);
            try
            {
                tempFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(hash + type);
                System.Diagnostics.Debug.WriteLine("File from cache:" + link);
            }
            catch (Exception)
            {
                tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(hash + type, CreationCollisionOption.OpenIfExists);
                await Helpers.Request.rh.DownloadFile(link, tempFile, page);
                System.Diagnostics.Debug.WriteLine("File to cache:" + link);
            }
            return tempFile;
        }

        public static async Task<string> giveMeString(string link)
        {
            string name = Any.CreateMD5(link) + ".json";
            StorageFile tempFile = null;
            try
            {
                tempFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(name);
                System.Diagnostics.Debug.WriteLine("String from cache:" + link);
            }
            catch (Exception)
            {
                tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(
                    tempFile,
                    await Helpers.Request.rh.Get(link)
                    );
                System.Diagnostics.Debug.WriteLine("String to cache:" + link);
            }

            return await FileIO.ReadTextAsync(tempFile);
        }

        public static async Task<string> updateAndGiveMeString(string link)
        {
            string name = Any.CreateMD5(link) + ".json";
            StorageFile tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            System.Diagnostics.Debug.WriteLine("Update string to cache:" + link);

            await FileIO.WriteTextAsync(
                tempFile,
                await Helpers.Request.rh.Get(link)
                );

            return await FileIO.ReadTextAsync(tempFile);
        }
    }
}
