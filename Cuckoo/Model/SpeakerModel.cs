using Cuckoo.Speaker;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Cuckoo.Model
{
    public class SpeakerModel : IDisposable
    {
        public List<ResourceStream> Resources { get; set; }
        public List<NumberStream> Numbers { get; set; }

        public SpeakerModel()
        {
            Resources = new List<ResourceStream>();
            Numbers = new List<NumberStream>();
        }

        public void Dispose()
        {
            Resources.ForEach(x => x.Dispose());
            Numbers.ForEach(x => x.Dispose());
        }

        public static SpeakerModel? Load(string languageSource, string languageSchemaSource)
        {
            SpeakerSchema? speakerSchema = getSpeakerSchema(languageSchemaSource);

            if (speakerSchema != null)
            {
                List<string> resourceNames = getResourceNames(speakerSchema.Extension, languageSource);
                if (resourceNames.Count > 0)
                {
                    SpeakerModel model = new SpeakerModel();
                    foreach (string name in resourceNames)
                        model.Resources.Add(new ResourceStream(name, Assembly.GetExecutingAssembly().GetManifestResourceStream($"{languageSource}.{name}{speakerSchema.Extension}")));

                    foreach (NumberSpeaker number in speakerSchema.Numbers)
                    {
                        for (int i = number.FromValue; i<=number.ToValue; i++)
                        {
                            List<Stream> numberStreams = new List<Stream>();
                            foreach (string sound in number.Sounds)
                            {
                                ResourceStream? resourceStream = getResourceStream(model.Resources, i, sound);                                
                                if (resourceStream != null && resourceStream.Stream != null)
                                    numberStreams.Add(resourceStream.Stream);
                                else
                                    break;
                            }
                            if (numberStreams.Count == number.Sounds.Count)
                                model.Numbers.Add(new NumberStream(i, numberStreams));
                            else
                                break;
                        }
                    }
                    return (model.Numbers.Count == speakerSchema.GetAllValuesCount()) ? model : null;
                }
            }
            return null;
        }

        private static SpeakerSchema? getSpeakerSchema(string languageSchemaSource)
        {
            using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(languageSchemaSource))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        if (!String.IsNullOrEmpty(json))
                            return JsonConvert.DeserializeObject<SpeakerSchema>(json);

                    }
                }
            }
            return null;
        }

        private static List<string> getResourceNames(string extension, string languageSource)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames()
                    .Where(x => x.StartsWith($"{languageSource}") && x.EndsWith(extension))
                    .Select(y => y.Replace($"{languageSource}.", String.Empty).Replace(extension, String.Empty)).ToList();
        }

        private static ResourceStream? getResourceStream(List<ResourceStream> resources, int number, string soundPattern)
        {
            string replacedSound = soundPattern;
            if (replacedSound.Contains("*N"))
                replacedSound = replacedSound.Replace("*N", $"{number}");
            if (replacedSound.Contains("*C"))
                replacedSound = replacedSound.Replace("*C", $"{(number%10)}");
            if (number > 20 && replacedSound.Contains("*T"))
                replacedSound = replacedSound.Replace("*T", $"{(number/10*10)}");
            return resources.FirstOrDefault(x => x.Key == replacedSound);
        }
    }
}
