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
        public enum Platform
        {
            iOS,
            Windows,
        }

        class BindOptions
        {
            public Platform Platform;
            public string ScriptEngineDir;
            public HashSet<string> AdapterSet;
            public HashSet<string> InterpSet;
        }

        static BindOptions options;

        private static HashSet<string> IgnoreAssemblySet = new HashSet<string>
        {
            "PureScript.dll","Adapter.gen.dll","UnityEngine.UnityAnalyticsModule.dll",
        };
        public static string ToolsetPath;


        static int Main(string[] args)
        {
            //TestWriter();
            //return;
      
            try
            {
                StartBinder(args);
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


            Console.WriteLine("start binder..");
            Directory.SetCurrentDirectory(Path.GetDirectoryName(configFile));

            var json = File.ReadAllText(configFile);
            options = JsonConvert.DeserializeObject<BindOptions>(json);

            string managedDir = Path.Combine(options.ScriptEngineDir, "Managed");
            string adapterDir = Path.Combine(options.ScriptEngineDir, "Adapter");
            ReplaceMscorlib("lib", managedDir);

            Binder.Init(Path.Combine(adapterDir, "glue"));
            CBinder.Init(Path.Combine(options.ScriptEngineDir, "generated"));
            AOTGenerater.Init(options.ScriptEngineDir);
            CSCGenerater.Init(ToolsetPath, adapterDir, managedDir, options.AdapterSet);

            foreach (var filePath in Directory.GetFiles(managedDir))
            {
                var file = Path.GetFileName(filePath);

                if (file.EndsWith(".dll") && !IgnoreAssemblySet.Contains(file))
                {
                    if (options.AdapterSet.Contains(file))
                    {
                        Binder.Bind(filePath);
                    }
                    else
                    {
                        if (!options.InterpSet.Contains(file))
                        {
                            Console.WriteLine("aot: " + file);
                            AOTGenerater.AddAOTAssembly(filePath);
                        }

                        if (file.StartsWith("UnityEngine."))
                        {
                            Console.WriteLine("bind icall: " + file);
                            CBinder.Bind(filePath);
                        }
                        else
                        {
                            if (options.InterpSet.Contains(file))
                                Console.WriteLine("Interpreter runtime: " + file);
                        }
                    }
                }
            }
            CBinder.End();
            Binder.End();
            CSCGenerater.End();
            AOTGenerater.End();

            foreach(var file in options.AdapterSet)
            {
                var path = Path.Combine(managedDir, file);
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        public static void ReplaceMscorlib(string libDir,string outDir)
        {
            OperatingSystem os = Environment.OSVersion;
            var srcDir = Path.Combine(libDir, os.Platform == PlatformID.MacOSX ? "iOS" : "win32");

            DirectoryInfo dir = new DirectoryInfo(srcDir);

            foreach (var fi in dir.GetFiles())
            {
                File.Copy(Path.Combine(srcDir, fi.Name), Path.Combine(outDir, fi.Name), true);
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
