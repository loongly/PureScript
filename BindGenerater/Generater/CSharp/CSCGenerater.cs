using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generater
{
    public class CSCGenerater
    {
        static string[] addtionFlag = new string[]
        {
            "-t:library",
            "-unsafe",
        };
        static string[] addtionRef = new string[]
        {
            "mscorlib.dll",
            "PureScript.dll",
        };

        private static string CSCPath = "csc";


        private static string[] AdapterSrc = new string[]
        {
            "glue/Binder.define.cs",
            "glue/Binder.funcser.cs",
            "Tools/CustomBinder.cs",
            "Tools/ObjectStore.cs",
        };

        private static string[] AdapterWrapperSrc = new string[]
        {
            "glue/Binder.define.cs",
            "glue/Binder.funcdeser.cs",
            "Tools/CustomBinder.cs",
            "Tools/ObjectStore.wrapper.cs",
            "Tools/ScriptEngine.cs",
            
        };

        public static CSCGenerater AdapterCompiler;
        public static CSCGenerater AdapterWrapperCompiler;
        private static string OutDir;
        private static string DllRefDir;
        private static string AdapterDir;
        private static HashSet<string> IgnoreRefSet = new HashSet<string>();
        private static Dictionary<string, CSCGenerater> WrapperDic = new Dictionary<string, CSCGenerater>();

        public static void Init(string cscDir,string adapterDir, string outDir,string dllRefDir, HashSet<string> ignoreRefSet)
        {
            CSCPath = Path.Combine(cscDir, Utils.IsWin32() ? "csc.exe":"csc") ;
            OutDir = outDir;
            DllRefDir = dllRefDir;
            AdapterDir = adapterDir;
            IgnoreRefSet = ignoreRefSet;
            AdapterCompiler = new CSCGenerater(Path.Combine(outDir, "Adapter.gen.dll"));
            
            foreach(var file in AdapterSrc)
                AdapterCompiler.AddSource(Path.Combine(adapterDir,file));

            SetWrapper("Adapter.wrapper.dll");
        }

        public static void SetWrapper(string dllName)
        {
            if (!WrapperDic.TryGetValue(dllName,out AdapterWrapperCompiler))
            {
                AdapterWrapperCompiler = new CSCGenerater(Path.Combine(OutDir, dllName));
                foreach (var file in AdapterWrapperSrc)
                    AdapterWrapperCompiler.AddSource(Path.Combine(AdapterDir, file));
                AdapterWrapperCompiler.AddDefine("WRAPPER_SIDE");
                if (!Utils.IsWin32())
                    AdapterWrapperCompiler.AddDefine("IOS");

                WrapperDic[dllName] = AdapterWrapperCompiler;
            }
        }

        public static void End()
        {
            AdapterCompiler.Gen();
            //AdapterWrapperCompiler.Gen();
            foreach(var wrapper in WrapperDic.Values)
            {
                wrapper.Gen();
            }
        }


        string outName;
        HashSet<string> refSet = new HashSet<string>();
        HashSet<string> srcSet = new HashSet<string>();
        HashSet<string> defineSet = new HashSet<string>();

        public CSCGenerater(string targetName)
        {
            outName = Path.GetFullPath(targetName);
        }

        public void AddReference(string target)
        {
            //if(!IgnoreRefSet.Contains(target))
            refSet.Add(target);
        }
        public void RemoveReference(string target)
        {
            //if(!IgnoreRefSet.Contains(target))
            refSet.Remove(target);
        }

        public void AddSource(string file)
        {
            var path = Path.GetFullPath(file);
            srcSet.Add(path);
        }

        public void AddDefine(string define)
        {
            defineSet.Add(define);
        }

        public void Gen()
        {
            var fName = $"{Path.GetFileName(outName)}.txt";
            using(var config = File.CreateText(fName))
            {
                config.WriteLine($"-out:{outName}");
                foreach (var flag in addtionFlag)
                    config.WriteLine(flag);
                foreach (var refFile in addtionRef)
                    config.WriteLine($"-r:{Path.Combine(DllRefDir, refFile)}");
                foreach (var refFile in refSet)
                    config.WriteLine($"-r:{Path.Combine(DllRefDir, refFile)}");
                foreach (var define in defineSet)
                    config.WriteLine($"-define:{define}");

                var netstandFile = Path.Combine(OutDir, "netstandard.dll");
                if(File.Exists(netstandFile))
                    config.WriteLine($"-r:{Path.Combine(DllRefDir, netstandFile)}");

                foreach (var src in srcSet)
                    config.WriteLine(src);
            }

            int res = Utils.RunCMD(CSCPath, new string[] { $"@{fName}" });
            if (res != 0)
                throw new Exception($"Run CSC with  {fName} error. ");
        }
        /*public void AddTypeForwardedTo(string typeName)
        {

        }*/
    }
}
