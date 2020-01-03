using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotTranslations.Models
{
    /// <summary>
    /// The ParseMode that you use for sending messages
    /// </summary>
    public enum ParseMode
    {
        /// <summary>
        /// No parse mode at all
        /// </summary>
        Default,

        /// <summary>
        /// ParseMode "markdown" or "MarkdownV2"
        /// </summary>
        Markdown,

        /// <summary>
        /// ParseMode "html"
        /// </summary>
        Html
    }
}
