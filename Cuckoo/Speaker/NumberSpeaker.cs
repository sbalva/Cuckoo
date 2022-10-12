using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Speaker
{
    public class NumberSpeaker
    {
        public int FromValue { get; set; }
        public int ToValue { get; set; }
        public List<string> Sounds { get; set; }

        public NumberSpeaker()
        {
            FromValue = int.MinValue;
            ToValue = int.MinValue;
            Sounds = new List<string>();
        }

        public int GetValuesCount()
        {
            if (FromValue == ToValue)
                return 1;
            return ToValue - FromValue + 1;
        }
    }
}
