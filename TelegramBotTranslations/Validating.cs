using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TelegramBotTranslations.Models;

namespace TelegramBotTranslations
{
    public sealed partial class BotTranslationManager
    {
        /// <summary>
        /// Validate an already existing language file.
        /// </summary>
        /// <param name="language">The language file to be validated.</param>
        /// <returns>Returns the validation results in the given parse mode.</returns>
        public string ValidateLanguage(string language)
        {
            var newFilePath = Path.Combine(FilesPath, language + ".xml");

            var fileErrors = new List<LanguageError>();
            ReloadLanguages();
            var langs = Languages.Select(x => x.Value);
            var master = Master;
            var file = new Language(newFilePath);

            CheckLanguageNode(file, fileErrors);
            TestLength(file, fileErrors);

            var error = langs.FirstOrDefault(x =>
                    (x.FileName == file.FileName && (x.Base != file.Base || x.Variant != file.Variant)) //check for same filename and mismatching base-variant
                    || (x.Base == file.Base && x.Variant == file.Variant && x.FileName != file.FileName) //check for same base-variant and mismatching filename
            );
            if (error != null)
            {
                //problem....
                fileErrors.Add(new LanguageError(file.FileName, "Language Node".ToBold(Parsemode),
                    $"ERROR: The following file partially matches the same language node. Please check the file name, and the language base and variant. Aborting.\n\n" + $"{error.FileName}.xml".ToBold(Parsemode) + $"\n" + "Base:".ToItalic(Parsemode) + $" {error.Base}\n" + "Variant:".ToItalic(Parsemode) + $" {error.Variant}", ErrorLevel.FatalError));
            }

            GetFileErrors(file, fileErrors, master);
            return OutputResult(file, fileErrors);
        }

        private void CheckLanguageNode(Language langfile, List<LanguageError> errors)
        {
            if (String.IsNullOrWhiteSpace(langfile.Base))
                errors.Add(new LanguageError(langfile.FileName, "Language Node".ToBold(Parsemode), "Base is missing", ErrorLevel.FatalError));
            if (String.IsNullOrWhiteSpace(langfile.Variant))
                errors.Add(new LanguageError(langfile.FileName, "Language Node".ToBold(Parsemode), "Variant is missing", ErrorLevel.FatalError));
        }

        private void TestLength(Language file, List<LanguageError> fileErrors)
        {
            var test = $"testing|-1001234567890|{file.Base ?? ""}|{file.Variant ?? ""}|t";
            var count = Encoding.UTF8.GetByteCount(test);
            if (count > 64)
                fileErrors.Add(new LanguageError(file.FileName, "Language Node".ToBold(Parsemode), "Base and variant are too long.", ErrorLevel.FatalError));
        }

        private void GetFileErrors(Language file, List<LanguageError> fileErrors, XDocument master)
        {
            var masterStrings = master.Descendants("string");

            foreach (var str in masterStrings)
            {
                var key = str.Attribute("key").Value;
                int.TryParse(str.Attributes().FirstOrDefault(x => x.Name == "maxlength")?.Value ?? "0", out int maxlength);
                //get the english string
                //get the locale values
                var masterString = GetString(key, master);
                var values = file.Doc.Descendants("string")
                        .FirstOrDefault(x => x.Attribute("key").Value == key)?
                        .Descendants("value");
                if (values == null)
                {
                    fileErrors.Add(new LanguageError(file.FileName, key, $"Values missing"));
                    continue;
                }
                //check master string for {#} values
                int vars = 0;
                for (int i = 0; i < 10; i++)
                    if (masterString.Contains("{" + i + "}"))
                        vars = i + 1;

                foreach (var value in values)
                {
                    for (int i = 0; i <= 10 - 1; i++)
                    {
                        if (!value.Value.Contains("{" + i + "}") && vars - 1 >= i)
                        {
                            //missing a value....
                            fileErrors.Add(new LanguageError(file.FileName, key, "Missing {" + i + "}", ErrorLevel.Error));
                        }
                        else if (value.Value.Contains("{" + i + "}") && vars - 1 < i)
                        {
                            fileErrors.Add(new LanguageError(file.FileName, key, "Extra {" + i + "}", ErrorLevel.Error));
                        }
                    }

                    if (maxlength != 0 && value.Value.Length > maxlength)
                    {
                        fileErrors.Add(new LanguageError(file.FileName, key, $"String is longer than {maxlength} character(s)", ErrorLevel.FatalError));
                    }
                }
            }
        }

        private string OutputResult(Language newFile, List<LanguageError> newFileErrors, Language curFile, List<LanguageError> curFileErrors, bool canUpload, ParseMode parseMode)
        {
            var result = "NEW FILE\n" + $"{newFile.FileName}.xml - ({(newFile.Base ?? "") + " " + (newFile.Variant ?? "")})".ToBold(Parsemode) + "\n";

            if (newFileErrors.Any(x => x.Level == ErrorLevel.Error))
            {
                result += "Errors:".ToItalic(Parsemode) + "\n";
                result = newFileErrors.Where(x => x.Level == ErrorLevel.Error).Aggregate(result, (current, fileError) => current + $"{fileError.Key}\n{fileError.Message}\n\n");
            }
            if (newFileErrors.Any(x => x.Level == ErrorLevel.MissingString))
            {
                result += "Missing Values:".ToItalic(Parsemode) + "\n";
                result = newFileErrors.Where(x => x.Level == ErrorLevel.MissingString).Aggregate(result, (current, fileError) => current + $"{fileError.Key}\n");
            }
            if (newFileErrors.Any(x => x.Level == ErrorLevel.FatalError))
            {
                result += "\nFatal errors:".ToBold(Parsemode) + "\n";
                result = newFileErrors.Where(x => x.Level == ErrorLevel.FatalError).Aggregate(result, (current, fileError) => current + $"{fileError.Key}\n{fileError.Message}\n\n");
            }
            if (newFileErrors.Count == 0)
            {
                result += "No errors".ToItalic(Parsemode) + "\n";
            }

            if (curFile != null)
            {
                result += "\n\n";
                result += "OLD FILE".ToBold(Parsemode) + $" (Last updated: {curFile.LatestUpdate.ToString("MMM dd")})\n{curFile.FileName}.xml - ({curFile.Base} {curFile.Variant})\n";
                result +=
                    $"Errors: {curFileErrors.Count(x => x.Level == ErrorLevel.Error)}\nMissing strings: {curFileErrors.Count(x => x.Level == ErrorLevel.MissingString)}";
            }
            else
            {
                result += "\n\n" + "No old file, this is a new language".ToBold(Parsemode);
                result += "\nPlease double check the filename, and the language base and variant, as you won't be able to change them.";
                result += $"\n" + "Base:".ToItalic(Parsemode) + $" {newFile.Base ?? ""}";
                if (!Directory.GetFiles(FilesPath, "*.xml").Select(x => new Language(x)).Any(x => x.Base == newFile.Base))
                    result += " " + "(NEW)".ToBold(Parsemode);
                result += $"\n" + "Variant:".ToItalic(Parsemode) + $" {newFile.Variant ?? ""}";
            }

            if (!canUpload) result += $"\n\n{"Fatal errors present, can't upload!".ToBold(parseMode)}";

            return result;
        }

        private string OutputResult(Language file, List<LanguageError> fileErrors)
        {
            var result = $"{file.FileName}.xml - ({(file.Base ?? "") + " " + (file.Variant ?? "")})".ToBold(Parsemode) + "\n";

            if (fileErrors.Any(x => x.Level == ErrorLevel.Error))
            {
                result += "Errors:".ToItalic(Parsemode) + "\n";
                result = fileErrors.Where(x => x.Level == ErrorLevel.Error).Aggregate(result, (current, fileError) => current + $"{fileError.Key}\n{fileError.Message}\n\n");
            }
            if (fileErrors.Any(x => x.Level == ErrorLevel.MissingString))
            {
                result += "Missing Values:".ToItalic(Parsemode) + "\n";
                result = fileErrors.Where(x => x.Level == ErrorLevel.MissingString).Aggregate(result, (current, fileError) => current + $"{fileError.Key}\n");
            }
            if (fileErrors.Any(x => x.Level == ErrorLevel.FatalError))
            {
                result += "\nFatal errors:".ToBold(Parsemode) + "\n";
                result = fileErrors.Where(x => x.Level == ErrorLevel.FatalError).Aggregate(result, (current, fileError) => current + $"{fileError.Key}\n{fileError.Message}\n\n");
            }
            if (fileErrors.Count == 0)
            {
                result += "No errors".ToItalic(Parsemode) + "\n";
            }

            result += $"\n" + "Base:".ToItalic(Parsemode) + $" {file.Base ?? ""}";
            result += $"\n" + "Variant:".ToItalic(Parsemode) + $" {file.Variant ?? ""}";

            return result;
        }
    }
}
