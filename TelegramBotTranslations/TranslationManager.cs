using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using TelegramBotTranslations.Models;

namespace TelegramBotTranslations
{
    /// <summary>
    /// The main class. Use a static <see cref="BotTranslationManager"/> and call its functions for the translations.
    /// </summary>
    public sealed partial class BotTranslationManager
    {
        private string BotToken { get; }
        private ParseMode Parsemode { get; }
        private string FilesPath { get; }
        private string TempPath { get; }
        private string MasterFileName { get; }
        private XDocument Master { get; set; }
        private Dictionary<string, Language> Languages { get; set; }
        private bool StrictErrors { get; }

        /// <summary>
        /// Create a new manager for translations.
        /// </summary>
        /// <param name="botToken">The token of your telegram bot. It's required to download the files that are supposed to be uploaded. See https://core.telegram.org/bots/api#getfile for more information.</param>
        /// <param name="filesPath">The path to save the used files to. Must be an existing directory the program has read and write access to.</param>
        /// <param name="tempPath">The path to save the temporary files to. Must be an existing directory the program has read and write access to.</param>
        /// <param name="masterFileName">Name of the master file (without file extension!). If a string is missing or cannot be formatted in a translation, the corresponding string from the master file will be used instead, and when uploading a language, it's validated against the current master file.</param>
        /// <param name="parsemode">The parse mode that your bot sends messages with. Methods that return strings for your bot to send will give the strings in this parse mode.</param>
        /// <param name="strictErrors">Whether missing or extra {#}'s in a string should make the bot disallow the upload.</param>
        public BotTranslationManager(string botToken, string filesPath, string tempPath, string masterFileName = "English", ParseMode parsemode = ParseMode.Default, bool strictErrors = false)
        {
            BotToken = botToken;
            Parsemode = parsemode;
            FilesPath = filesPath;
            TempPath = tempPath;
            MasterFileName = masterFileName;
            StrictErrors = strictErrors;

            ReloadLanguages();
        }

        /// <summary>
        /// Update the language files from the directory. Can be called after manual changes to the directory, doesn't need to be called after uploading a file through the <see cref="PrepareUploadLanguage"/> and <see cref="UploadLanguage"/> methods.
        /// </summary>
        public void ReloadLanguages()
        {
            var dict = new Dictionary<string, Language>();

            foreach (var file in Directory.GetFiles(FilesPath, "*.xml", SearchOption.TopDirectoryOnly))
            {
                var filename = Path.GetFileNameWithoutExtension(file);
                var xdoc = XDocument.Load(file);
                dict.Add(filename, new Language(filename, xdoc, Path.Combine(TempPath, filename + ".xml")));
            }

            Languages = dict;
            Master = dict.First(x => x.Key == MasterFileName).Value.Doc;
        }

        /// <summary>
        /// Get the Language file with the given file name
        /// </summary>
        /// <param name="fileName">The file name of the language file to get</param>
        /// <returns>the language file with this file name</returns>
        public Language GetLanguage(string fileName)
        {
            if (!Languages.ContainsKey(fileName)) throw new Exception($"No language file with filename {fileName} found.");
            return Languages[fileName];
        }

        /// <summary>
        /// Get a list of all currently available language bases
        /// </summary>
        /// <returns>a list of all currently available language bases</returns>
        public List<string> GetLanguageBases()
        {
            return Languages.Select(x => x.Value.Base).Distinct().ToList();
        }

        /// <summary>
        /// Get a list of all variants. If <paramref name="languageBase"/> is specified, only variants of that base will be returned.
        /// </summary>
        /// <param name="languageBase">The base to search languages. If this is null, all languages will be returned.</param>
        /// <returns>A list of all variants. If <paramref name="languageBase"/> is specified, only variants of that base will be returned.</returns>
        public List<Language> GetLanguageVariants(string languageBase = null)
        {
            if (languageBase == null) return Languages.Select(x => x.Value).ToList();
            return Languages.Select(x => x.Value).Where(x => x.Base == languageBase).ToList();
        }
    }
}
