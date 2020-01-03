using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotTranslations.Exceptions
{
    /// <summary>
    /// The <see cref="Exception"/> that is thrown when a string couldn't be formatted with the given arguments
    /// </summary>
    public class StringFormattingException : Exception
    {
        /// <summary>
        /// The Key of the string that was attempted to be formatted
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// The file name of the Language in which the string was got
        /// </summary>
        public string Language { get; }
        /// <summary>
        /// The arguments that were passed and couldn't be formatted in that string.
        /// </summary>
        public object[] Arguments { get; }

        internal StringFormattingException(string key, string language, object[] arguments)
            : base($"The string with the key {key} couldn't be formatted with arguments: {(arguments.Count() == 0 ? "None" : string.Join(", ", arguments))} (in both language {language} and the master file).")
        {
            Key = key;
            Language = language;
            Arguments = arguments;
        }

        internal StringFormattingException(string key, string language, object[] arguments, Exception innerException)
            : base($"The string with the key {key} couldn't be formatted with arguments: {(arguments.Count() == 0 ? "None" : string.Join(", ", arguments))} (in both language {language} and the master file).", innerException)
        {
            Key = key;
            Language = language;
            Arguments = arguments;
        }
    }
}
