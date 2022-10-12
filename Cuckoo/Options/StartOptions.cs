using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cuckoo.Model;

namespace Cuckoo.Options
{
    public class StartOptions
    {
        public TimerTypes TimerType { get; set; }
        public SpeakLanguages SpeakLanguage { get; set; }
        public BeepSounds Beep { get; set; }
        public int Interval { get; set; }
        public string Patterrn { get; set; }
        public bool OnTop { get; set; }

        public StartOptions() : this(TimerTypes.Clock, SpeakLanguages.EN, BeepSounds.Beep, 20, SpeakPattern.HHMMSS.Display, false)
        {
        }

        public StartOptions(TimerTypes timerType, SpeakLanguages speakLanguage, BeepSounds beep, int interval, string pattern, bool onTop)
        {
            TimerType = timerType;
            SpeakLanguage = speakLanguage;
            Beep = beep;
            Interval = interval;
            Patterrn = pattern;
            OnTop = onTop;
        }
    }
}
