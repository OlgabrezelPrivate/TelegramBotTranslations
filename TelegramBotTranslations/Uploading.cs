using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBotTranslations
{
    public sealed partial class BotTranslationManager
    {
        /// <summary>
        /// Downloads the file to the temporary language folder and validates it against the current master file.
        /// </summary>
        /// <param name="document">The document message containing the file that should be uploaded.</param>
        /// <returns>Returns a string with the validation results, the given parse mode will be applied to it.</returns>
        public string PrepareUploadLanguage(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var fileid = document.FileId;
            var filename = document.FileName;

            var fileT = Api.GetFileAsync(fileid);
            fileT.Wait();
            var file = fileT.Result;

            var uri = $"https://api.telegram.org/file/bot{BotToken}/{file.FilePath}";
            var newFilePath = Path.Combine(TempPath, filename);
            using (var client = new WebClient())
            {
                client.DownloadFile(new Uri(uri), newFilePath);
            }


            //ok, we have the file.  Now we need to determine the language, scan it and the original file.
            var newFileErrors = new List<LanguageError>();
            //first, reload existing files to program
            ReloadLanguages();

            var langs = Languages.Select(x => new LangFile(x.Key, x.Value, TempPath));
            var master = Master;
            var newFile = new LangFile(newFilePath);

            //make sure it has a complete langnode
            CheckLanguageNode(newFile, newFileErrors);

            //test the length
            TestLength(newFile, newFileErrors);

            //check uniqueness
            var error = langs.FirstOrDefault(x =>
                    (x.FileName == newFile.FileName && (x.Base != newFile.Base || x.Variant != newFile.Variant)) //check for same filename and mismatching base-variant
                    || (x.Base == newFile.Base && x.Variant == newFile.Variant && x.FileName != newFile.FileName) //check for same base-variant and mismatching filename
            );
            if (error != null)
            {
                //problem....
                newFileErrors.Add(new LanguageError(newFile.FileName, "Language Node".ToBold(Parsemode),
                    $"ERROR: The following file partially matches the same language node. Please check the file name, and the language base and variant. Aborting.\n\n" + $"{error.FileName}.xml".ToBold(Parsemode) + $"\n" + "Base:".ToItalic(Parsemode) + $" {error.Base}\n" + "Variant:".ToItalic(Parsemode) + $" {error.Variant}", ErrorLevel.FatalError));
            }

            //get the errors in it
            GetFileErrors(newFile, newFileErrors, master);

            //need to get the current file
            var curFile = langs.FirstOrDefault(x => x.FileName == newFile.FileName);
            var curFileErrors = new List<LanguageError>();

            if (curFile != null)
            {
                //test the length
                TestLength(curFile, curFileErrors);

                //get the errors in it
                GetFileErrors(curFile, curFileErrors, master);
            }

            //return the validation result
            return OutputResult(newFile, newFileErrors, curFile, curFileErrors);
        }

        /// <summary>
        /// Upload a language. The language must have been prepared with <see cref="PrepareUploadLanguage(Document)"/> before.
        /// </summary>
        /// <param name="fileName">The language to be uploaded.</param>
        /// <returns>Returns a message about the status of the uploading, in the given parse mode.</returns>
        public string UploadLanguage(string fileName)
        {
            string msg = "Moving file to language directory...\n";

            try
            {
                msg += "Checking paths for duplicate language file...\n";
                var newFilePath = Path.Combine(TempPath, fileName + ".xml");
                var copyToPath = Path.Combine(FilesPath, fileName + ".xml");

                //get the new files language
                var doc = XDocument.Load(newFilePath);

                var newFileLang = new
                {
                    Base = doc.Descendants("language").First().Attribute("base").Value,
                    Variant = doc.Descendants("language").First().Attribute("variant").Value
                };

                //check for existing file
                var langs = Directory.GetFiles(FilesPath).Select(x => new LangFile(x)).ToList();
                var lang = langs.FirstOrDefault(x => x.FileName == fileName);
                if (lang != null)
                {
                    //            
                }
                else
                {
                    lang = langs.FirstOrDefault(x => x.Base == newFileLang.Base && x.Variant == newFileLang.Variant && x.FileName != fileName);
                    if (lang != null)
                    {
                        msg += $"Found duplicate language (matching base and variant) with filename {Path.GetFileNameWithoutExtension(lang.FileName)}\n";
                        msg += "Aborting!".ToBold(Parsemode);
                        return msg;
                    }
                }


                System.IO.File.Copy(newFilePath, copyToPath, true);
                msg += "File copied to bot.\n";
                ReloadLanguages();
                msg += "Language files refreshed.\n";
                msg += "\n" + "Operation complete.".ToBold(Parsemode);
                return msg;
            }
            catch
            {
                return msg;
            }
        }
    }
}
