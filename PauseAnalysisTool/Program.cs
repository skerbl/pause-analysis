using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Schema;

namespace PauseAnalysisTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage error. Please specify a file containing valid keylog data created by InputLog 7.");
                return;
            }

            string inputPath = args[0];
            string tempFileName = string.Empty;

            string extension = Path.GetExtension(inputPath).Split().First();
            string fileName = Path.GetFileNameWithoutExtension(inputPath);

            try
            {
                tempFileName = Path.GetTempFileName();
                FileInfo fileInfo = new FileInfo(tempFileName);
                fileInfo.Attributes = FileAttributes.Temporary;

                File.Copy(inputPath, tempFileName, true);

                //Remove the hex value of the illegal backspace and CTRL characters. The XML parser doesn't like that.
                ReplaceInFile(tempFileName, "&#x8;", "BACKSPACE");
                ReplaceInFile(tempFileName, "&#xc;", "CTRL");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("The file you specified doesn't exist: " + ex.Message);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to create temporary file or set its attributes: " + ex.Message);
            }

            PauseAnalysis pauseAnalysis = new PauseAnalysis(tempFileName);

            try
            {
                pauseAnalysis.ValidateFile();
                Console.WriteLine("XML schema valid.");
                pauseAnalysis.ParseData();

                pauseAnalysis.FilterData();

                pauseAnalysis.ReformatData();

                pauseAnalysis.ConvertToCsv(fileName);

                // TODO: Collapse the repeated log events for held down keys into one event.
                // TODO: Find best way to include session information. Write to a new CSV?
            }
            catch (XmlSchemaException ex)
            {
                Console.WriteLine("The XML file you entered is not valid: " + ex.Message);
            }
        }

        /// <summary>
        /// Find and replace all occurances of a string in a file with another string.
        /// </summary>
        /// <param name="filePath">Full path and file name.</param>
        /// <param name="searchText">The string to be replaced.</param>
        /// <param name="replaceText">The replacement string.</param>
        public static void ReplaceInFile(string filePath, string searchText, string replaceText)
        {

            var content = string.Empty;
            using (StreamReader reader = new StreamReader(filePath))
            {
                content = reader.ReadToEnd();
                reader.Close();
            }

            content = Regex.Replace(content, searchText, replaceText);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(content);
                writer.Close();
            }
        }
    }
}
