using BimsyncCLI.Services;
using BimsyncCLI.Services.DelegatingHandlers;
using BimsyncCLI.Services.HttpServices;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Spectre.Console.Rendering;

namespace BimsyncCLI
{
    [HelpOption("--help")]
    public abstract class bimsyncCmdBase
    {
        protected IBimsyncClient _bimsyncClient;
        protected AuthenticationService _authenticationService;
        protected SettingsService _settingsService;
        protected ILogger _logger;
        protected IConsole _console;

        [Option(CommandOptionType.SingleValue, ShortName = "f", LongName = "output-format", Description = "Select the output format", ValueName = "table|json|xml", ShowInHelpText = true)]
        public string OutputFormat { get; set; } = "json";

        [Option(CommandOptionType.SingleValue, ShortName = "o", LongName = "output", Description = "Output the result to a file", ValueName = "output file", ShowInHelpText = true)]
        public string OutputFile { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "", LongName = "xslt", Description = "xslt input file for transformation", ValueName = "xslt file", ShowInHelpText = true)]
        public string XSLTFile { get; set; }

        protected string FileNameSuffix { get; set; }

        protected virtual Task<int> OnExecute(CommandLineApplication app)
        {
            return Task.FromResult(0);
        }

        protected string ProfileFolder
        {
            get
            {
                return $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.Create)}\\.istrada\\";
            }
        }


        protected String SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        private string EncryptKey
        {
            get
            {
                int keyLen = 32;
                string key = Environment.UserName;
                if (key.Length > keyLen)
                {
                    key = key.Substring(0, keyLen);
                }
                else if (key.Length < keyLen)
                {
                    int len = key.Length;
                    for (int i = 0; i < keyLen - len; i++)
                    {
                        key += ((char)(65 + i)).ToString();
                    }
                }
                return key;
            }
        }

        protected string Encrypt(string text)
        {
            var keyString = EncryptKey;
            var key = Encoding.UTF8.GetBytes(keyString);
            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }
                        var iv = aesAlg.IV;
                        var decryptedContent = msEncrypt.ToArray();
                        var result = new byte[iv.Length + decryptedContent.Length];
                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);
                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        protected string Decrypt(string cipherText)
        {
            var keyString = EncryptKey;
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - 16];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            var key = Encoding.UTF8.GetBytes(keyString);
            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                    return result;
                }
            }
        }

        protected void OnException(Exception ex)
        {
            OutputException(ex);
            _logger.LogError(ex.Message);
            _logger.LogDebug(ex, ex.Message);
        }

        protected void OutputJson(object data, string[] columns = null)
        {
            switch (OutputFormat.ToLowerInvariant())
            {
                case "table":
                    OutputTable(data, columns);
                    break;
                case "json":

                    JsonSerializerOptions options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };

                    string jsonOutput = JsonSerializer.Serialize(data, options);
                    Output(jsonOutput);
                    break;
                case "xml":

                    XDocument xdoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

                    using (var writer = xdoc.CreateWriter())
                    {
                        XmlSerializer x = new XmlSerializer(data.GetType());

                        x.Serialize(writer, data);
                    }
                    OutputXml(xdoc);
                    break;
                default:
                    OutputError("format not supported");
                    break;
            }
        }

        protected void OutputTable(object data, string[] columnNames)
        {
            IList collection = data as IList;
            List<object> objects = new List<object>();

            if (collection != null)
            {
                foreach (var item in collection)
                {
                    objects.Add(item);
                }
            }
            else
            {
                objects.Add(data);
            }

            // Create a table
            Table table = new Table();

            // Create the list of column name if no input
            if (columnNames == null)
            {
                Type itemType = objects[0].GetType();
                columnNames = itemType.GetProperties().Select(p => p.Name).ToArray();
            }

            // Add some columns
            foreach (string columnName in columnNames)
            {
                table.AddColumn(columnName);
            }

            // Add some rows
            foreach (object item in objects)
            {
                List<string> columnsValues = new List<string>();

                foreach (string columnName in columnNames)
                {
                    Type itemType = item.GetType();

                    // Match properties with column names
                    List<string> propertiesNames = itemType.GetProperties().Select(p => p.Name).ToList();
                    string propertyName = propertiesNames.Where(propertiesName => propertiesName.ToLower() == columnName.ToLower().Replace(" ", "")).FirstOrDefault();

                    if (propertyName == null) continue;

                    object value = itemType.GetProperty(propertyName).GetValue(item, null);

                    if (value == null)
                    {
                        columnsValues.Add("");
                    }
                    else if (value.GetType() == typeof(DateTime))
                    {
                        DateTime date = (DateTime)value;
                        columnsValues.Add(date.ToString("MMMM dd, yyyy"));
                    }
                    else
                    {
                        columnsValues.Add(value.ToString());
                    }
                }

                table.AddRow(columnsValues.ToArray());
            }

            Output(table);

        }

        protected void OutputXml(XDocument data)
        {
            if (string.IsNullOrEmpty(XSLTFile))
            {
                Output(data.ToString());
            }
            else
            {
                XsltSettings settings = new XsltSettings(true, true);
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(XSLTFile, settings, null);
                XsltArgumentList argList = new XsltArgumentList();
                object ext = null; // new XSLTExtension();
                argList.AddExtensionObject("urn:istrada-cli", ext);
                var sb = new StringBuilder();
                using (var writer = new StringWriter(sb))
                {
                    xslt.Transform(data.CreateReader(ReaderOptions.None), argList, writer);
                    writer.Flush();
                }
                Output(sb.ToString());
            }
        }

        protected void Output(string data)
        {
            if (!string.IsNullOrEmpty(OutputFile))
            {
                OutputToFile(data);
            }
            else
            {
                OutputToConsole(data);
            }
        }

        protected void Output(Table table)
        {
            if (!string.IsNullOrEmpty(OutputFile))
            {
                string outputString = "";
                foreach (TableRow tableRow in table.Rows)
                {
                    foreach (Markup markup in tableRow)
                    {
                        List<string> segmentStrings = markup.GetSegments(AnsiConsole.Console).Select(s => s.Text).ToList();
                        segmentStrings.RemoveAt(0);
                        segmentStrings.RemoveAt(segmentStrings.Count - 1);
                        string line = segmentStrings.Aggregate((concat, str) => $"{concat}&{str}");
                        outputString = outputString + line.Replace("Fetching all projects...\n                          ", "");
                    }
                }
                OutputToFile(outputString);
            }
            else
            {
                // Render the table to the console
                AnsiConsole.Write(table);
            }
        }

        protected void OutputToFile(string data)
        {
            if (IsValidPath(OutputFile))
            {
                File.WriteAllText(string.IsNullOrEmpty(FileNameSuffix) ? OutputFile : OutputFile.Replace("*", FileNameSuffix), data);
            }
            else
            {
                throw new Exception("The output path is not valid");
            }
        }

        private bool IsValidPath(string path, bool allowRelativePaths = false)
        {
            bool isValid = true;

            try
            {
                string fullPath = Path.GetFullPath(path);

                if (allowRelativePaths)
                {
                    isValid = Path.IsPathRooted(path);
                }
                else
                {
                    string root = Path.GetPathRoot(path);
                    isValid = string.IsNullOrEmpty(root.Trim(new char[] { '\\', '/' })) == false;
                }
            }
            catch
            {
                // isValid = false;
                throw new Exception("The output path is not valid");
            }

            return isValid;
        }


        protected void OutputToConsole(string data)
        {
            AnsiConsole.MarkupInterpolated($"{data}");
        }

        protected void OutputError(string message)
        {
            AnsiConsole.Markup($"[bold white on red]{message}[/]");
        }

        protected void OutputException(Exception ex)
        {
            AnsiConsole.WriteException(ex,
                ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes |
                ExceptionFormats.ShortenMethods | ExceptionFormats.ShowLinks);
        }
    }
}