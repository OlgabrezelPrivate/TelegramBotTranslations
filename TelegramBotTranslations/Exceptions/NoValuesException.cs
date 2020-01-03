using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotTranslations.Exceptions
{
    /// <summary>
    /// The <see cref="Exception"/> that is thrown when a string has no value
    /// </summary>
    public class NoValuesException : Exception
    {
        /// <summary>
        /// The key of the string that has no value
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// The file name of the language in which that string was attempted to get
        /// </summary>
        public string Language { get; }

        internal NoValuesException(string key, string language)
            : base($"The string with the key {key} has no values (in both language {language} and the master file).")
        {
            Key = key;
            Language = language;
        }

        internal NoValuesException(string key, string language, Exception innerException)
            : base($"The string with the key {key} has no values (in both language {language} and the master file).", innerException)
        {
            Key = key;
            Language = language;
        }
    }
}
