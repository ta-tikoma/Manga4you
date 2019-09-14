using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Manga.Helpers.Save
{
    class Mangas
    {
        public static string Export(ObservableCollection<VModels.Manga.InHistory> MangaList)
        {
            string content = "";
            foreach (VModels.Manga.InHistory manga in MangaList)
            {
                content += manga.Manga.ToJson() + System.Environment.NewLine;
            }
            return content;
        }

        public static void Import(ref ObservableCollection<VModels.Manga.InHistory> MangaList, string content)
        {
            MangaList.Clear();
            string[] lines = content.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
            for (int index = MangaList.Count; index < lines.Length; index++)
            {
                if (lines[index].Length > 0)
                {
                    JsonObject jo = null;
                    try
                    {
                        jo = JsonValue.Parse(lines[index]).GetObject();
                    }
                    catch (Exception)
                    {
                    }

                    if (jo != null)
                    {
                        MangaList.Add(
                            new VModels.Manga.InHistory(
                                new Models.Manga(jo)
                            )
                        );
                    }
                }
            }
            Save(MangaList);
        }

        public static bool isBusy = false;

        public static void Save(ObservableCollection<VModels.Manga.InHistory> MangaList)
        {
            if (isBusy)
            {
                return;
            }
            isBusy = true;

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            int index = 0;

            for (; index < MangaList.Count; index++)
            {
                //System.Diagnostics.Debug.WriteLine("Save: manga_" + index + ": " + MangaList[index].Manga.Name);
                localSettings.Values["manga_" + index] = MangaList[index].Manga.ToJson();
            }

            // remove old
            while (localSettings.Values.ContainsKey("manga_" + index))
            {
                //System.Diagnostics.Debug.WriteLine("Remove: manga_" + index);
                localSettings.Values.Remove("manga_" + index);
                index++;
            }

            isBusy = false;
        }

        public static void SaveFirst(Models.Manga manga)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["manga_0"] = manga.ToJson();
        }

        public static void Load(ref ObservableCollection<VModels.Manga.InHistory> MangaList)
        {
            if (isBusy)
            {
                return;
            }
            isBusy = true;

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            int index = 0;
            while (localSettings.Values.ContainsKey("manga_" + index))
            {
                //System.Diagnostics.Debug.WriteLine("Load: manga_" + index);

                string json = localSettings.Values["manga_" + index].ToString();
                JsonObject jo = JsonValue.Parse(json).GetObject();

                MangaList.Add(
                    new VModels.Manga.InHistory(
                        new Models.Manga(jo)
                    )
                );
                index++;
            }

            isBusy = false;
        }
    }
}
