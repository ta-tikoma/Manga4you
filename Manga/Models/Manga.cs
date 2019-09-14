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
    class Manga
    {
        public string SiteHash { get; set; } = "";
        public string Name { get; set; } = "";
        public string Link { get; set; } = "";

        // Zoom
        public float Zoom = 1;
        public bool AutoZoom = true;

        // Chapters
        public int ChaptersCount { get; set; } = 0;
        public int CurrentChapter { get; set; } = 0;

        // Pages
        public int PagesCount { get; set; } = 0;
        public int CurrentPage { get; set; } = 0;

        // Site
        public Site Site {
            get {
                return Site.GetByHash(SiteHash);
            }
        }

        // Constructor
        public Manga(string name, string link, string site_hash)
        {
            Name = name;
            Link = link;
            SiteHash = site_hash;
        }

        public Manga(JsonObject jo)
        {
            if (jo.ContainsKey("name"))
            {
                Name = jo.GetNamedString("name");
            }

            if (jo.ContainsKey("link"))
            {
                Link = jo.GetNamedString("link");
            }

            if (jo.ContainsKey("site_hash"))
            {
                SiteHash = jo.GetNamedString("site_hash");
            }

            // Zoom
            if (jo.ContainsKey("zoom"))
            {
                Zoom = (float)jo.GetNamedNumber("zoom");
            }

            if (jo.ContainsKey("auto_zoom"))
            {
                AutoZoom = jo.GetNamedBoolean("auto_zoom");
            }

            // Chapters
            if (jo.ContainsKey("chapters_count"))
            {
                ChaptersCount = (int) jo.GetNamedNumber("chapters_count");
            }

            if (jo.ContainsKey("current_chapter"))
            {
                CurrentChapter = (int)jo.GetNamedNumber("current_chapter");
            }

            // Pages
            if (jo.ContainsKey("pages_count"))
            {
                PagesCount = (int)jo.GetNamedNumber("pages_count");
            }

            if (jo.ContainsKey("current_page"))
            {
                CurrentPage = (int)jo.GetNamedNumber("current_page");
            }
        }

        // Load
        public async Task<ObservableCollection<Chapter>> ChaptersLoad(bool fresh = false)
        {
            // контент
            string res = await Helpers.Cache.giveMeString(Link, fresh);

            // регулярки
            List<string> links = Helpers.Regular.GetValuesByJO(
                Site.GetJsonObject(Site.JO_TYPE_CHAPTER, Site.JO_PATH_LINK),
                res
            );

            List<string> names = Helpers.Regular.GetValuesByJO(
                Site.GetJsonObject(Site.JO_TYPE_CHAPTER, Site.JO_PATH_NAME),
                res
            );

            // формируем ответ
            ObservableCollection<Chapter> chapters = new ObservableCollection<Chapter>();
            for (int i = 0; i < links.Count; i++)
            {
                chapters.Insert(0, new Chapter() { Name = names[i], Link = links[i], SiteHash = SiteHash });
            }

            // обновляем количество глав
            ChaptersCount = chapters.Count;

            return chapters;
        }

        // S
        public string ToJson()
        {
            JsonObject jo = new JsonObject();
            jo.SetNamedValue("name", JsonValue.CreateStringValue(Name));
            jo.SetNamedValue("link", JsonValue.CreateStringValue(Link));
            jo.SetNamedValue("site_hash", JsonValue.CreateStringValue(SiteHash));
            jo.SetNamedValue("zoom", JsonValue.CreateNumberValue(Zoom));
            jo.SetNamedValue("auto_zoom", JsonValue.CreateBooleanValue(AutoZoom));
            jo.SetNamedValue("chapters_count", JsonValue.CreateNumberValue(ChaptersCount));
            jo.SetNamedValue("current_chapter", JsonValue.CreateNumberValue(CurrentChapter));
            jo.SetNamedValue("pages_count", JsonValue.CreateNumberValue(PagesCount));
            jo.SetNamedValue("current_page", JsonValue.CreateNumberValue(CurrentPage));
            return jo.ToString();
        }

        public bool Compare(Manga manga)
        {
            return (manga.SiteHash == SiteHash) && (manga.Link == Link);
        }
    }
}
