using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TelegramBotTranslations.Exceptions;

namespace TelegramBotTranslations
{
    public sealed partial class BotTranslationManager
    {
        /// <summary>
        /// Get the translation of a string in a certain language.
        /// </summary>
        /// <param name="key">The key of the string to get.</param>
        /// <param name="language">The language to get the key in.</param>
        /// <param name="arguments">The arguments that the string will be formatted with. Every {#} inside the string will be replaced by arguments[#]</param>
        /// <returns></returns>
        public string GetTranslation(string key, string language, params object[] arguments)
        {
            var lang = Languages.FirstOrDefault(x => x.Key == language).Value ?? Master;

            var str = lang.Descendants("string").FirstOrDefault(x => x.Attribute("key").Value == key)
                ?? Master.Descendants("string").FirstOrDefault(x => x.Attribute("key").Value == key);

            if (str == null)
            {
                throw new NoStringException(key, language);
            }

            var values = str.Descendants("value");
            if (values.Count() == 0)
            {
                throw new NoValuesException(key, language);
            }

            var vals = values.Select(x => x.Value).ToList();
            var val = vals[new Random().Next(vals.Count)];

            try
            {
                return String.Format(val.Replace("\\n", "\n"), arguments);
            }
            catch
            {
                str = Master.Descendants("string").FirstOrDefault(x => x.Attribute("key").Value == key);

                if (str == null)
                {
                    throw new NoStringException(key, language);
                }

                values = str.Descendants("value");
                if (values.Count() == 0)
                {
                    throw new NoValuesException(key, language);
                }

                vals = values.Select(x => x.Value).ToList();
                val = vals[new Random().Next(vals.Count)];

                try
                {
                    return String.Format(vals.First().Replace("\\n", "\n"), arguments);
                }
                catch (FormatException)
                {
                    throw new StringFormattingException(key, language, arguments);
                }
            }
        }

        private string GetString(string key, XDocument file)
        {
            var strings = file.Descendants("string").FirstOrDefault(x => x.Attribute("key").Value == key);
            var values = strings.Descendants("value");
            return values.First().Value;
        }
    }
}
