using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Model
{
    public class NumberStream : IDisposable
    {
        public int Value { get; set; }
        public List<Stream> Streams { get; set; }

        public NumberStream() : this(int.MinValue, new List<Stream>())
        { 
        }

        public NumberStream(int value, List<Stream> streams)
        {
            Value = value;
            Streams = streams;
        }

        public void Dispose()
        {
            if (Streams.Count > 0)
                Streams.ForEach(x => 
                { 
                    x.Close();
                    x.Dispose();
                });
        }
    }
}
