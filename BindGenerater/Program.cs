using Generater;
using Mono.Cecil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BindGenerater
{
    class Program
    {

        class BindOptions
        {
            public string ScriptEngineDir;
            public HashSet<string> AdapterSet;
            public HashSet<string> InterpSet;
            public HashSet<string> Entry;
        }
        public enum BindTarget
        {
            Adapter,
            All,
        }


        static BindOptions options;

        private static HashSet<string> IgnoreAssemblySet = new HashSet<string>
        {
            "PureScript.dll","Adapter.gen.dll","UnityEngine.UnityAnalyticsModule.dll",
        };
        public static string ToolsetPath;
        public static BindTarget Mode;

        static int Main(string[] args)
        {
            //TestWriter();
            //return;
            Console.WriteLine("OS: " + Environment.OSVersion);

            //StartBinder(args);
            //StartTestBinder();
             try
             {
                 StartBinder(args);
                 //StartTestBinder();
                 Utils.Log("Binder All Done..");
             }
             catch(Exception e)
             {
                 Console.Error.WriteLine(e.ToString());
                 return 2;
             }
            return 0;
        }

        static void StartBinder(string[] args)
        {
            if (args.Length < 2)
                return;
            var configFile = args[0];
            ToolsetPath = args[1];
            if (args.Length >= 3)
                Mode = (BindTarget)Enum.Parse(typeof(BindTarget), args[2]);
            else
                Mode = BindTarget.All;


            Console.WriteLine("start binder..");
            Directory.SetCurrentDirectory(Path.GetDirectoryName(configFile));

            var json = File.ReadAllText(configFile);
            options = JsonConvert.DeserializeObject<BindOptions>(json);

            string managedDir = Path.Combine(options.ScriptEngineDir, "Managed");
            string orignDir = Path.Combine(options.ScriptEngineDir, "Managed_orign");
            string adapterDir = Path.Combine(options.ScriptEngineDir, "Adapter");

            Utils.CopyDir(orignDir,managedDir, ".dll");
            ReplaceMscorlib("lib", managedDir);

            Binder.Init(Path.Combine(adapterDir, "glue"));
            CSCGenerater.Init(ToolsetPath, adapterDir,managedDir, options.AdapterSet);
            CBinder.Init(Path.Combine(options.ScriptEngineDir, "generated"));
            AOTGenerater.Init(options.ScriptEngineDir);

            var assemblySet = new HashSet<string>();
            foreach(var entry in options.Entry)
            {
                Utils.CollectMonoAssembly(entry, orignDir, options.AdapterSet, assemblySet);
            }

            options.AdapterSet.UnionWith(assemblySet.Where(assem => assem.StartsWith("UnityEngine.")));

            foreach (var assembly in options.AdapterSet)
            {
                if (IgnoreAssemblySet.Contains(assembly))
                    continue;

                var filePath = Path.Combine(orignDir, assembly);

                if(File.Exists(filePath))
                {
                    Binder.Bind(filePath);
                }
                
            }
            Binder.End();
            CSCGenerater.End();

            if (Mode == BindTarget.All)
            {
               // CBinder.Bind(CSCGenerater.AdapterWrapperCompiler.outName);

                foreach (var filePath in Directory.GetFiles(managedDir))
                {
                    var file = Path.GetFileName(filePath);
                    if (file.EndsWith(".dll") && !IgnoreAssemblySet.Contains(file) && !options.InterpSet.Contains(file))
                    {
                        if (file.StartsWith("UnityEngine."))
                            CBinder.Bind(filePath);

                        Console.WriteLine("aot: " + file);
                        AOTGenerater.AddAOTAssembly(filePath);
                    }
                }

                CBinder.End();
                AOTGenerater.End();
            }


        }

        public static void ReplaceMscorlib(string libDir, string outDir)
        {
            var srcDir = Path.Combine(libDir, Utils.IsWin32() ? "win32" : "iOS");

            DirectoryInfo dir = new DirectoryInfo(srcDir);

            foreach (var fi in dir.GetFiles())
            {
                var tarPath = Path.Combine(outDir, fi.Name);
                if (File.Exists(tarPath) || fi.Name == "netstandard.dll") // netstandard reserved for unity2020 build bug
                    File.Copy(Path.Combine(srcDir, fi.Name), tarPath, true);
            }
        }



        static void StartTestBinder()
        {
            string managedDir = "Managed";

            TestBinder.Init(@"test\");

            foreach (var filePath in Directory.GetFiles(managedDir))
            {
                var file = Path.GetFileName(filePath);

                if (file.EndsWith(".dll") && !IgnoreAssemblySet.Contains(file))
                {
                    TestBinder.TestBind(filePath);
                }
            }
            TestBinder.End();
        }

        //Try to modify
        static void TestWriter()
        {
            using (new CS(new CodeWriter(File.CreateText("TestWriter.txt"))))
            {
                CS.Writer.Start("Test");
                CS.Writer.WriteLine("1");
                CS.Writer.WriteLine("3");
                CS.Writer.WritePreviousLine("2");

                CS.Writer.CreateLinePoint("//aa");

                CS.Writer.WriteLine("4");

                var writer2 = new CodeWriter(File.CreateText("TestWriter2.txt"));
                using (new CS(writer2))
                {
                    CS.Writer.WriteLine("write Test2 file..");
                }

                CS.Writer.WriteLine("5");

                using (new LP(CS.Writer.GetLinePoint("//aa")))
                {
                    CS.Writer.Start("LinePointTest");
                    CS.Writer.WriteLine("aa 1");
                    CS.Writer.WriteLine("aa 2");
                    var resolverRes = new ClassResolver(null).Box("testObj");
                    CS.Writer.WriteLine($"return {resolverRes}");
                    CS.Writer.End();
                }

                CS.Writer.WriteLine("6");

                var cResolverRes = new Generater.C.StringResolver(null, false).Box("testStr");
                CS.Writer.WriteLine($"return {cResolverRes}");

                CS.Writer.End();

                using (new CS(writer2))
                {
                    CS.Writer.WriteLine("write Test2 file once more...", false);
                }
            }
        }
    }
}
