using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generater
{
    public class Config
    {
        public HashSet<string> IgnoreAssemblySet;
        public HashSet<string> CSharpIgnorTypes;
        public HashSet<string> ForceRetainTypes;
        public HashSet<string> StripUsing;
        public HashSet<string> ICallIgnorTypes;

        private static Config _instance;
        public static Config Instance
        {
            get {
                if(_instance == null)
                {
                    var json = File.ReadAllText("binder.json");
                    _instance = JsonConvert.DeserializeObject<Config>(json);
                }
                return _instance;
            }
        }
    }
}
