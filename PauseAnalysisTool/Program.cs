using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace PauseAnalysisTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage error. Please specify a valid filename.");
                return;
            }

            string fileName = args[0];
            string tempFileName = string.Empty;

            try
            {
                tempFileName = Path.GetTempFileName();
                FileInfo fileInfo = new FileInfo(tempFileName);
                fileInfo.Attributes = FileAttributes.Temporary;

                File.Copy(fileName, tempFileName, true);

                //Remove the hex value of the illegal backspace character. The XML parser doesn't like that.
                ReplaceInFile(tempFileName, " &#x8;", "");
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
                Console.WriteLine("XML schema validated.");
                pauseAnalysis.ParseData();
                Console.WriteLine("Data parsed successfully.");
            }
            catch (XmlSchemaException ex)
            {
                Console.WriteLine("The XML file you entered is not valid: " + ex.Message);
            }
            
            

            Console.Read();
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
