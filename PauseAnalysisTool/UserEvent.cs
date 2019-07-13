using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PauseAnalysisTool
{
    public enum EventType { MouseClick, KeyPress, Focus };

    class UserEvent
    {
        public int Id { get; private set; }
        public string Type { get; private set; }
        public string Name { get; private set; }
        public string Value { get; private set; }
        public int StartTime { get; private set; }
        public int EndTime { get; private set; }

        public UserEvent(int id, string type, string name, string value, int startTime, int endTime)
        {
            Id = id;
            Type = type;
            Name = name;
            Value = value;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
