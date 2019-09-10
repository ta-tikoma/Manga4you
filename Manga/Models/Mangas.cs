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

namespace Manga.Models
{
    class Mangas
    {
        public static string Export(ObservableCollection<Manga> MangaList)
        {
            string content = "";
            foreach (Manga manga in MangaList)
            {
                content += manga.ToJson() + System.Environment.NewLine;
            }
            return content;
        }

        public static void Import(ref ObservableCollection<Manga> MangaList, string content)
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
                        MangaList.Add(new Manga(jo, index));
                    }
                }
            }
            Save(MangaList);
        }

        public static void Save(ObservableCollection<Manga> MangaList)
        {
            int index = 0;
            for (; index < MangaList.Count; index++)
            {
                MangaList[index].index_for_save = index;
            }

            // remove old
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            while (localSettings.Values.ContainsKey("manga_" + index))
            {
                localSettings.Values.Remove("manga_" + index);
                index++;
            }
        }

        public static void Load(ref ObservableCollection<Manga> MangaList)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            int index = 0;
            while (localSettings.Values.ContainsKey("manga_" + index))
            {
                MangaList.Add(
                    new Manga(
                        JsonValue.Parse(localSettings.Values["manga_" + index].ToString()).GetObject(),
                        index
                        )
                    );
                index++;
            }
        }
    }
}
