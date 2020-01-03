using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace TelegramBotTranslations.Models
{
    internal class File
    {
        internal string FileId { get; set; }
        internal string FileUniqueId { get; set; }
        internal Nullable<int> FileSize { get; set; }
        internal string FilePath { get; set; }

        internal static File GetFile(string botToken, string fileId)
        {
            using (var client = new HttpClient())
            {
                var str = client.GetStringAsync($"https://api.telegram.org/bot{botToken}/getFile?file_id={fileId}").Result;
                return JsonConvert.DeserializeObject<File>(str);
            }
        }
    }
}
