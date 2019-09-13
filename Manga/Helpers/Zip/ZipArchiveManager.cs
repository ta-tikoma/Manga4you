using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Manga.Helpers
{
    class ZipArchiveManager
    {
        public static async Task ZipFolder(StorageFolder folder, StorageFile file)
        {
            using (var zipMemoryStream = await file.OpenStreamForWriteAsync())
            {
                using (var zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create))
                {
                    await AddZipFolderToEntry(zipArchive, folder, "");
                }
            }
        }

        private static async Task<bool> AddZipFolderToEntry(ZipArchive zipArchive, StorageFolder folder, string entryFirst)
        {
            IReadOnlyList<StorageFile> filesToCompress = await folder.GetFilesAsync();

            foreach (StorageFile fileToCompress in filesToCompress)
            {
                byte[] buffer = (await FileIO.ReadBufferAsync(fileToCompress)).ToArray();
                ZipArchiveEntry entry = zipArchive.CreateEntry(entryFirst + fileToCompress.Name);
                using (Stream entryStream = entry.Open())
                {
                    await entryStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            var childrenFolder = await folder.GetFoldersAsync();
            foreach (var storageFolder in childrenFolder)
            {
                await AddZipFolderToEntry(zipArchive, storageFolder, entryFirst + storageFolder.Name + "/");
            }

            return true;
        }
    }
}
