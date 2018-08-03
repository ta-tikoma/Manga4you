using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Helpers
{
    class Any
    {
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static async Task ShareBy(Models.Site site)
        {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Body = "I want to add site to Manga4you for everyone\n" +
                "\n" + site.name +
                "\n" + site.search_link +
                "\n" + site.search_post +
                "\n" + site.search_regexp +
                "\n" + site.chapters_link +
                "\n" + site.chapters_regexp +
                "\n" + site.pages_link +
                "\n" + site.pages_regexp +
                "\n"
                ;

            var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient("toradora011@live.ru");
            emailMessage.To.Add(emailRecipient);

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }
    }
}
