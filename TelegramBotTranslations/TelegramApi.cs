using System;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace TelegramBotTranslations
{
    internal class TelegramApi
    {
        private string Token { get; }
        public TelegramApi(string BotToken)
        {
            Token = BotToken;
        }

        public void DownloadFile(string FileId, string DestinationPath)
        {
            using (var httpclient = new HttpClient())
            {
                string uri = $"https://api.telegram.org/bot{Token}/getFile?file_id={FileId}";
                string str = httpclient.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;

                var file = JsonConvert.DeserializeObject<TGFileResponse>(str);

                if (file == null || file.Result == null || file.Ok == false || file.Result.FileId == null || file.Result.FilePath == null)
                {
                    throw new Exception("Couldn't parse file object returned by Telegram's getFile API method");
                }

                uri = $"https://api.telegram.org/file/bot{Token}/{file.Result.FilePath}";
                var stream = httpclient.GetStreamAsync(uri).Result;
                stream.Seek(0, SeekOrigin.Begin);

                using (var filestream = File.Create(DestinationPath))
                {
                    stream.CopyTo(filestream);
                }
            }
        }

        private class TGFileResponse
        {
            [JsonProperty(PropertyName = "ok")]
            public bool Ok { get; set; }

            [JsonProperty(PropertyName = "result")]
            public TgFile Result { get; set; }
        }

        private class TgFile
        {
            [JsonProperty(PropertyName = "file_id")]
            public string FileId { get; set; }

            [JsonProperty(PropertyName = "file_size")]
            public int FileSize { get; set; }

            [JsonProperty(PropertyName = "file_path")]
            public string FilePath { get; set; }
        }
    }
}
