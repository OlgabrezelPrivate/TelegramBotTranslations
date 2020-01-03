using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TelegramBotTranslations.Models
{
    /// <summary>
    /// Represents a language file
    /// </summary>
    public class Language
    {
        /// <summary>
        /// The language's base as in the XML file
        /// </summary>
        public string Base { get; set; }
        /// <summary>
        /// The language's variant as in the XML file
        /// </summary>
        public string Variant { get; set; }
        /// <summary>
        /// The file name of the XML file
        /// </summary>
        public string FileName { get; set; }
        internal XDocument Doc { get; set; }
        /// <summary>
        /// The time when the language file was updated the last time
        /// </summary>
        public DateTime LatestUpdate { get; }

        internal Language(string path)
        {
            Doc = XDocument.Load(path);
            Base = Doc.Descendants("language").First().Attribute("base")?.Value;
            Variant = Doc.Descendants("language").First().Attribute("variant")?.Value;
            FileName = Path.GetFileNameWithoutExtension(path);
            LatestUpdate = System.IO.File.GetLastWriteTimeUtc(path);
        }

        internal Language(string xmlName, XDocument xmlDoc, string tempPath)
        {
            Doc = xmlDoc;
            Base = Doc.Descendants("language").First().Attribute("base")?.Value;
            Variant = Doc.Descendants("language").First().Attribute("variant")?.Value;
            FileName = xmlName;
            LatestUpdate = System.IO.File.GetLastWriteTimeUtc(Path.Combine(tempPath, xmlName, ".xml"));
        }
    }
}
