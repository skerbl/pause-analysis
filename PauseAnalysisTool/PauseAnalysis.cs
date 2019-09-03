using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private List<@event> eventList;
        private List<UserEvent> simplifiedList;
        private int firstEventTime;
        private int videoOffset = 5766;     // Keystroke logging starts at 5 seconds and 23 frames

        public PauseAnalysis(string fileName)
        {
            this.fileName = fileName;
            simplifiedList = new List<UserEvent>();
        }

        /// <summary>
        /// Validates the specified .idfx file against the InputLog XML schema.
        /// </summary>
        /// <exception cref="XmlSchemaException"></exception>
        public void ValidateFile()
        {
            keyLog = new log();
            eventList = new List<@event>();
            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add("", Environment.CurrentDirectory + @"\Schema_InputLog.xsd");
            xmlDocument = XDocument.Load(fileName);

            xmlDocument.Validate(schema, (s, e) =>
            {
                throw new XmlSchemaException(e.Message);
            });
        }

        /// <summary>
        /// Reads the contents of the specified .idfx file into memory.
        /// </summary>
        public void ParseData()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(log));

            keyLog = serializer.Deserialize(new FileStream(fileName, FileMode.Open)) as log;
        }

        /// <summary>
        /// Removes unwanted actions from the dataset, e.g. certain mouse actions or actions happening in windows other than MS Word.
        /// </summary>
        public void FilterData()
        {
            Console.WriteLine("Please enter the exact title of the process Window (e.g. \"MyDocument.docx - Word\":");
            string windowName = Console.ReadLine();
            firstEventTime = int.Parse(keyLog.@event[0].part.startTime);
            eventList = keyLog.@event.ToList();
            FilterMouseMovement();
            FilterByFocus(windowName);

            Console.WriteLine("Filter applied. Removed " + (keyLog.@event.Length - eventList.Count) + " items.");
            Console.WriteLine("Final list contains " + eventList.Count + " items.");
        }

        /// <summary>
        /// Converts the keylog data into a simplified format
        /// </summary>
        public void ReformatData()
        {
            simplifiedList.Clear();

            int lastEventTime = 0;

            foreach (@event item in eventList)
            {
                int tId = int.Parse(item.id);
                string tType = item.type;
                string tValue = item.type == "keyboard" ? item.part.Items[1].ToString() : item.part.Items[0].ToString();

                switch (item.part.Items[0].ToString())
                {
                    case "VK_LSHIFT":
                        tValue = "L_SHIFT";
                        break;
                    case "VK_RSHIFT":
                        tValue = "R_SHIFT";
                        break;
                    case "VK_LCONTROL":
                        tValue = "L_CTRL";
                        break;
                    case "VK_RCONTROL":
                        tValue = "R_CTRL";
                        break;
                    case "VK_DELETE":
                        tValue = "DELETE";
                        break;
                    case "VK_SPACE":
                        tValue = "SPACE";
                        break;
                    case "VK_UP":
                        tValue = "UP_ARROW";
                        break;
                    case "VK_DOWN":
                        tValue = "DOWN_ARROW";
                        break;
                    case "VK_LEFT":
                        tValue = "LEFT_ARROW";
                        break;
                    case "VK_RIGHT":
                        tValue = "RIGHT_ARROW";
                        break;
                    case "VK_OEM_7":
                        tValue = "ä";
                        break;
                    case "VK_OEM_3":
                        tValue = "ö";
                        break;
                    case "VK_OEM_1":
                        tValue = "ü";
                        break;
                    case "VK_OEM_4":
                        tValue = "ß";
                        break;

                    // TODO: Come up with a better way to deal with the pesky " character.
                    //       This is just a dirty and insufficient fix (will mess up the number 2).
                    case "VK_2":
                        tValue = "\\\"";
                        break;
                    default:
                        break;
                }

                int tStart = int.Parse(item.part.startTime);
                int tEnd = int.Parse(item.part.endTime);
                int tVideo = tStart - firstEventTime + videoOffset;

                UserEvent userEvent = new UserEvent(tId, tType, tValue, tStart, tEnd, tVideo);

                if (lastEventTime == 0)
                {
                    userEvent.deltaTime = lastEventTime;
                }
                else
                {
                    userEvent.deltaTime = tStart - lastEventTime;
                }

                lastEventTime = tStart;

                simplifiedList.Add(userEvent);

                // TODO: Figure out a way to correctly parse characters like umlauts etc.
                // It works in my case, because there are no capitalized umauts, but
                // in a more general case these these things need to be fixed.
                // Also, this switch-case statements is ugly and cumbersome.
            }
        }

        /// <summary>
        /// Exports data as a .csv file.
        /// </summary>
        /// <param name="fileName">The name of the original file without file extension.</param>
        public void ConvertToCsv(string fileName)
        {
            const string header = "ID|Type|Value|Start|End|VideoTimestamp|DeltaTime";
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header);

            foreach (UserEvent item in simplifiedList)
            {
                sb.AppendLine(item.ToString());
            }

            File.WriteAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName + ".csv"),
                sb.ToString());
        }

        /// <summary>
        /// Exports the keylog metadata as a separate .csv file. Appending it to the main data file would
        /// result in a "messy" import into Excel.
        /// </summary>
        /// <param name="fileName">The name of the original file without file extension.</param>
        public void WriteMetadata(string fileName)
        {
            const string header = "Key|Value";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header);

            foreach (entry k in keyLog.session)
            {
                sb.AppendLine(k.key + "|" + k.value);
            }

            File.WriteAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName + "_metadata" + ".csv"),
                sb.ToString());
        }

        /// <summary>
        /// Removes mouse movement and scrolling from eventList
        /// </summary>
        private void FilterMouseMovement()
        {
            int counter = 0;

            foreach (@event item in eventList.ToList())
            {
                if (item.type == "mouse" && item.part.type != "click")
                {
                    eventList.Remove(item);
                    counter++;
                }
            }
        }

        /// <summary>
        /// Removes everything that does not happen in a specific window
        /// </summary>
        private void FilterByFocus(string windowTitle)
        {
            int counter = 0;
            bool inFocus = false;

            foreach (@event item in eventList.ToList())
            {
                if (!inFocus)
                {
                    if (item.type == "focus" && item.part.title == windowTitle)
                    {
                        inFocus = true;
                    }

                    eventList.Remove(item);
                    counter++;
                }
                else
                {
                    if (item.type == "focus" && item.part.title != windowTitle)
                    {
                        inFocus = false;
                        eventList.Remove(item);
                        counter++;
                    }
                }
            }
        }
    }
}
