using System.Collections.Generic;
using System.Linq;

namespace TelegramBotTranslations
{
    public sealed partial class BotTranslationManager
    {
        /// <summary>
        /// Get a full list of existing language bases.
        /// </summary>
        /// <returns>Returns a full list of existing language bases.</returns>
        public List<string> GetLanguageBases()
        {
            return Languages.Select(x => new LangFile(x.Key, x.Value, TempPath)).GroupBy(x => x.Base).Select(x => x.First().Base).ToList();
        }

        /// <summary>
        /// Get a List of language informations, either for a certain base or for all language files.
        /// </summary>
        /// <param name="languageBase">Optional. The language base you want to get the filenames and variants for. If nothing is specified, a list of all languages' infos be returned.</param>
        /// <returns>A list of languages for the specified language base or for all language files.</returns>
        public List<Models.Language> GetLanguageVariants(string languageBase = null)
        {
            var list = new List<Models.Language>();
            if (languageBase == null)
            {
                foreach (var langfile in Languages.Select(x => new LangFile(x.Key, x.Value, TempPath)))
                {
                    list.Add(new Models.Language(langfile.Base, langfile.Variant, langfile.FileName, langfile.LangCode));
                }
                return list;
            }

            foreach (var langfile in Languages.Select(x => new LangFile(x.Key, x.Value, TempPath)).Where(x => x.Base == languageBase))
            {
                list.Add(new Models.Language(languageBase, langfile.Variant, langfile.FileName, langfile.LangCode));
            }
            return list;
        }

        /// <summary>
        /// Get a language information.
        /// </summary>
        /// <param name="fileName">The file name (without extension!) of the file you want to get</param>
        /// <returns>A language object containing informations about the language</returns>
        public Models.Language GetLanguage(string fileName)
        {
            var lang = Languages.First(x => x.Key == fileName);
            var langfile = new LangFile(lang.Key, lang.Value, TempPath);
            return new Models.Language(langfile.Base, langfile.Variant, langfile.FileName, langfile.LangCode);
        }
    }
}
