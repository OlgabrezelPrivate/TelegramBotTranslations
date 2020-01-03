using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramBotTranslations
{
    /// <summary>
    /// The main class. Use a static <see cref="BotTranslationManager"/> and call its functions for the translations.
    /// </summary>
    public sealed partial class BotTranslationManager
    {
        private string BotToken { get; }
        private TelegramBotClient Api { get; }
        private ParseMode Parsemode { get; }
        private string FilesPath { get; }
        private string TempPath { get; }
        private string MasterFileName { get; }
        private XDocument Master { get; set; }
        private Dictionary<string, XDocument> Languages { get; set; }

        /// <summary>
        /// Create a new manager for translations.
        /// </summary>
        /// <param name="botToken">The token of your telegram bot. It's required to download the files that are supposed to be uploaded. See https://core.telegram.org/bots/api#getfile for more information.</param>
        /// <param name="filesPath">The path to save the used files to. Must be an existing directory the program has read and write access to.</param>
        /// <param name="tempPath">The path to save the temporary files to. Must be an existing directory the program has read and write access to.</param>
        /// <param name="masterFileName">Name of the master file (without file extension!). If a string is missing or cannot be formatted in a translation, the corresponding string from the master file will be used instead, and when uploading a language, it's validated against the current master file.</param>
        /// <param name="parsemode">The parse mode that your bot sends messages with. Methods that return strings for your bot to send will give the strings in this parse mode.</param>
        public BotTranslationManager(string botToken, string filesPath, string tempPath, string masterFileName = "English", ParseMode parsemode = ParseMode.Default)
        {
            BotToken = botToken;
            Api = new TelegramBotClient(BotToken);
            Parsemode = parsemode;
            FilesPath = filesPath;
            TempPath = tempPath;
            MasterFileName = masterFileName;

            ReloadLanguages();
        }

        /// <summary>
        /// Update the language files from the directory. Can be called after manual changes to the directory, doesn't need to be called after uploading a file through the <see cref="PrepareUploadLanguage"/> and <see cref="UploadLanguage"/> methods.
        /// </summary>
        public void ReloadLanguages()
        {
            var dict = new Dictionary<string, XDocument>();

            foreach (var file in Directory.GetFiles(FilesPath, "*.xml", SearchOption.TopDirectoryOnly))
            {
                dict.Add(Path.GetFileNameWithoutExtension(file), XDocument.Load(file));
            }

            Languages = dict;
            Master = dict.First(x => x.Key == MasterFileName).Value;
        }
    }
}
