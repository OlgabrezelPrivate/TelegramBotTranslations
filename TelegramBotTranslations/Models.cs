using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TelegramBotTranslations
{
    internal class LangFile
    {
        public string Base { get; set; }
        public string Variant { get; set; }
        public string FileName { get; set; }
        public string LangCode { get; set; }
        public XDocument Doc { get; set; }
        public DateTime LatestUpdate { get; }

        public LangFile(string path)
        {
            Doc = XDocument.Load(path);
            Base = Doc.Descendants("language").First().Attribute("base")?.Value;
            Variant = Doc.Descendants("language").First().Attribute("variant")?.Value;
            FileName = Path.GetFileNameWithoutExtension(path);
            LangCode = Doc.Descendants("language").First().Attribute("langcode")?.Value;
            LatestUpdate = File.GetLastWriteTimeUtc(path);
        }

        public LangFile(string xmlName, XDocument xmlDoc, string tempPath)
        {
            Doc = xmlDoc;
            Base = Doc.Descendants("language").First().Attribute("base")?.Value;
            Variant = Doc.Descendants("language").First().Attribute("variant")?.Value;
            FileName = xmlName;
            LangCode = Doc.Descendants("language").First().Attribute("langcode")?.Value;
            LatestUpdate = File.GetLastWriteTimeUtc(Path.Combine(tempPath, xmlName, ".xml"));
        }
    }

    internal class LanguageError
    {
        public string File { get; set; }
        public string Key { get; set; }
        public string Message { get; set; }
        public ErrorLevel Level { get; set; }

        public LanguageError(string file, string key, string message, ErrorLevel level = ErrorLevel.MissingString)
        {
            File = file;
            Key = key;
            Message = message;
            Level = level;
        }
    }

    internal enum ErrorLevel
    {
        DuplicatedString, MissingString, Error, FatalError, ExtraString
    }
}
