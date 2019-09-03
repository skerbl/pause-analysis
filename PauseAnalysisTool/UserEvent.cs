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
        public string Value { get; private set; }
        public int StartTime { get; private set; }
        public int EndTime { get; private set; }
        public int VideoTimestamp { get; private set; }

        public int deltaTime { get; set; }

        public UserEvent(int id, string type, string value, int startTime, int endTime, int videoTimestamp)
        {
            Id = id;
            Type = type;
            Value = value;
            StartTime = startTime;
            EndTime = endTime;
            VideoTimestamp = videoTimestamp;
        }

        public override string ToString()
        {
            return Id.ToString() + "|" + Type + "|" + Value + "|" + StartTime.ToString() + "|" + EndTime.ToString() + "|" + VideoTimestamp.ToString() + "|" + deltaTime;
        }
    }
}
