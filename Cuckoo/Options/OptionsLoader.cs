using Cuckoo.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Options
{
    public class OptionsLoader
    {
        private static readonly string subKeyName = "Cuckoo";
        private static readonly string valueName = "StartOptions";

        public static void SaveStartOptions(StartOptions options)
        {
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(subKeyName);
            rk.SetValue(valueName, JsonConvert.SerializeObject(options));
            rk.Close();
        }

        public static StartOptions LoadStartOptions()
        {
            StartOptions? options = new StartOptions();
            Microsoft.Win32.RegistryKey? rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subKeyName);
            if (rk != null)
            {
                object? value = rk.GetValue(valueName);
                if (value != null)
                {
                    options = JsonConvert.DeserializeObject<StartOptions>(value.ToString());
                    if (options == null)
                        options = new StartOptions();
                }
            }
            return (options != null) ? options : new StartOptions();
        }

    }
}
