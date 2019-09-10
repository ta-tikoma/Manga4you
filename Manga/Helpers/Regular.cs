using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Manga.Helpers
{
    class Regular
    {
        // формируем список json объекту должен обязательно содержать маску и набор регулярок
        public static List<string> GetValuesByJO(JsonObject jo, string res)
        {
            string mask = jo.GetNamedString("mask");
            JsonObject regexp = jo.GetNamedObject("regexp");

            // размер списка
            int count = 0;

            // вычисляем все значения
            Dictionary<string, List<string>> dictionaryWithSingleValue = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> dictionaryWithManyValue = new Dictionary<string, List<string>>();
            foreach (string key in regexp.Keys)
            {
                List<string> values = GetValuesByRegexp(regexp.GetNamedString(key), res);

                if (values.Count == 1)
                {
                    dictionaryWithSingleValue.Add(key, values);
                }
                else
                {
                    dictionaryWithManyValue.Add(key, values);
                    if (values.Count > count)
                    {
                        count = values.Count;
                    }
                }
            }

            // обогащаем маску одиночными значениями
            foreach (string key in dictionaryWithSingleValue.Keys)
            {
                mask = mask.Replace("#" + key + "#", dictionaryWithSingleValue[key][0]);
            }

            // создаем результирующий список
            List<string> list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                list.Add(mask);
            }


            // обогощаем результат значениями из списка
            foreach (string  key in dictionaryWithManyValue.Keys)
            {
                List<string> values = dictionaryWithManyValue[key];
                for (int i = 0; i < values.Count; i++)
                {
                    if (list.Count >= i) // на выход из диапазона
                    {
                        list[i] = list[i].Replace("#" + key + "#", values[i]);
                    }

                    System.Diagnostics.Debug.WriteLine("--------------------");
                    System.Diagnostics.Debug.WriteLine("list[i]:" + list[i]);
                }
            }

            return list;
        }

        // находим список значения по regexp в контенте res
        private static List<string> GetValuesByRegexp(string regexp, string res)
        {
            List<string> list = new List<string>();

            try
            {
                Regex regex = new Regex(regexp);
                MatchCollection matches = regex.Matches(res);
                // во всех найденых результатах
                foreach (Match match in matches)
                {
                    GroupCollection collection = match.Groups;
                    for (int i = 0; i < collection.Count; i++)
                    {
                        Group group = collection[i];
                        // выделяе декларацию value
                        if (regex.GroupNameFromNumber(i) == "value")
                        {
                            string value = group.Value;
                            value = Regex.Unescape(value).Trim();
                            value = Regex.Replace(value, @"\t|\n|\r", "");
                            value = Regex.Replace(value, @"  ", "");

                            // и сохраняем в список
                            list.Add(
                                value//Regex.Replace(Regex.Unescape(group.Value), "<.*?>", String.Empty)
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Regular:GetValues: " + ex.Message);
            }

            return list;
        }
    }
}
