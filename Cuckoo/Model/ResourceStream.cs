using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Model
{
    public class ResourceStream : IDisposable
    {
        public string Key { get; set; }
        public Stream? Stream { get; set; }

        public ResourceStream() : this(String.Empty, null)
        {
        }

        public ResourceStream(string key, Stream? stream)
        {
            Key = key;
            Stream = stream;
        }

        public void Dispose()
        {
            Stream?.Close();
            Stream?.Dispose();
        }
    }
}
