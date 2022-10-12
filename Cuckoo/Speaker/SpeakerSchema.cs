using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Speaker
{
    public class SpeakerSchema
    {
        public string Language { get; set; }
        public string Extension { get; set; }
        public List<NumberSpeaker> Numbers { get; set; }

        public SpeakerSchema()
        {
            Language = String.Empty;
            Extension = String.Empty;
            Numbers = new List<NumberSpeaker>();
        }

        public int GetAllValuesCount()
        {
            return Numbers.Sum(x => x.GetValuesCount());
        }
    }
}
