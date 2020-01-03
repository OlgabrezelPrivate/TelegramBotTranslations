namespace TelegramBotTranslations.Models
{
    /// <summary>
    /// Represents a language file
    /// </summary>
    public sealed class Language
    {
        /// <summary>
        /// The language's base as in the XML file
        /// </summary>
        public string Base { get; }
        /// <summary>
        /// The language's variant as in the XML file
        /// </summary>
        public string Variant { get; }
        /// <summary>
        /// The file name of the XML file
        /// </summary>
        public string FileName { get; }
        /// <summary>
        /// The language's IETF code
        /// </summary>
        public string LangCode { get; }

        internal Language(string Base, string Variant, string FileName, string LangCode)
        {
            this.Base = Base;
            this.Variant = Variant;
            this.FileName = FileName;
            this.LangCode = LangCode;
        }
    }
}