using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PauseAnalysisTool
{
    class PauseAnalysis
    {
        private string fileName;
        private XDocument xmlDocument;
        private log keyLog;

        public PauseAnalysis(string fileName)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// Validates the specified .idfx file against the InputLog XML schema.
        /// </summary>
        /// <exception cref="XmlSchemaException"></exception>
        public void ValidateFile()
        {
            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add("", Environment.CurrentDirectory + @"\Schema_InputLog.xsd");
            xmlDocument = XDocument.Load(fileName);
            bool validationErrors = false;

            xmlDocument.Validate(schema, (s, e) =>
            {
                validationErrors = true;
                throw new XmlSchemaException(e.Message);
            });
        }

        /// <summary>
        /// Reads the contents of the specified .idfx file into memory.
        /// </summary>
        public void ParseData()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(log));

            log mylog = serializer.Deserialize(new FileStream(fileName, FileMode.Open)) as log;

            /*
            if (mylog != null)
            {
                Console.WriteLine("Parsing succsessful.");

                for (int i = 0; i < mylog.session.Length; i++)
                {
                    Console.WriteLine(mylog.session[i].key + "\t" + mylog.session[i].value);
                }

                for (int i = 0; i < mylog.@event.Length; i++)
                {
                    int duration = int.Parse(mylog.@event[i].part.endTime) - int.Parse(mylog.@event[i].part.startTime);

                    Console.WriteLine(mylog.@event[i].id + "\t" + mylog.@event[i].type + "\t" + duration);
                }
            }
            */
        }

        /// <summary>
        /// Removes unwanted actions from the dataset, e.g. certain mouse actions or actions happening in windows other than MS Word.
        /// </summary>
        public void FilterData()
        {
            List<UserEvent> userEvents = new List<UserEvent>();

            foreach (var action in keyLog.@event)
            {
                
            }
        }

        private void FilterMouseMovement()
        {
            
        }
    }
}
