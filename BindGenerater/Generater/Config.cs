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
        /// <summary>
        /// 黑名单，排除一些不需要绑定的程序集，如：UnityEngine.UnityAnalyticsModule.dll
        /// </summary>
        public HashSet<string> IgnoreAssemblySet;

        /// <summary>
        /// 黑名单，排除一些不支持的类型，这里是匹配规则，即可以排除整个命名空间，如：UnityEditor / UnityEngine.TestTools
        /// </summary>
        public HashSet<string> CSharpIgnorTypes;

        /// <summary>
        /// 白名单，强制保留整个类型在Mono内执行，如：UnityEngine.Transform
        /// </summary>
        public HashSet<string> ForceRetainTypes;

        /// <summary>
        /// 排除掉没用的using，如：UnityEngine.Internal / UnityEngine.Scripting.APIUpdating
        /// </summary>
        public HashSet<string> StripUsing;

        /// <summary>
        /// 黑名单，ICall绑定时不支持的类型，如：System.Reflection
        /// </summary>
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
