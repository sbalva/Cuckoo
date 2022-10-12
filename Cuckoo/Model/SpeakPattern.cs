using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Model
{
    public class SpeakPattern
    {
        public bool Hours { get; set; }
        public bool Minutes { get; set; }
        public bool Seconds { get; set; }
        public string Display 
        {
            get 
            { 
                List<string> strings = new List<string>();
                if (Hours)
                    strings.Add("HH");
                if (Minutes)
                    strings.Add("MM");
                if (Seconds)
                    strings.Add("SS");
                return String.Join(":",strings); 
            }
        }

        public SpeakPattern() : this(true, true, true)
        { 
        }

        public SpeakPattern(bool hours, bool minutes, bool seconds)
        {
            Hours=hours;
            Minutes=minutes;
            Seconds=seconds;
        }

        public override string ToString()
        {
            return Display;
        }

        public static SpeakPattern HHMMSS { get { return new SpeakPattern(); } }
        public static SpeakPattern HHMM { get { return new SpeakPattern(true, true, false); } }
        public static SpeakPattern MMSS { get { return new SpeakPattern(false, true, true); } }
    }
}
