using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotTranslations.Exceptions
{
    /// <summary>
    /// The <see cref="Exception"/> that is thrown when a string that's attempted to be got doesn't exist
    /// </summary>
    public class NoStringException : Exception
    {
        /// <summary>
        /// The key of the missing string
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// The file name of the language in which the string was attempted to be got
        /// </summary>
        public string Language { get; }

        internal NoStringException(string key, string language)
            : base($"No string with key {key} could be found (in both language {language} and the master file).")
        {
            Key = key;
            Language = language;
        }

        internal NoStringException(string key, string language, Exception innerException)
            : base($"No string with key {key} could be found (in both language {language} and the master file).", innerException)
        {
            Key = key;
            Language = language;
        }
    }
}
