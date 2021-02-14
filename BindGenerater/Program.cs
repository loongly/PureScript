using Generater;
using Mono.Cecil;
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
        private static HashSet<string> IgnoreAssemblySet = new HashSet<string>
        {
            // "UnityEngine.UI.dll", "UnityEngine.Networking.dll" ,"UnityEngine.Timeline.dll" ,"UnityEditor.VR.dll",//UnityExtensions
            "UnityEngine.UnityAnalyticsModule.dll",
        };

        static void Main(string[] args)
        {
            Utils.Log("Hello world.");
            //TestWriter();
            //return;

            string fileDir = @"..\..\EngineLib\full\";
            Binder.Init(@"..\..\glue\");
            CBinder.Init(@"..\..\..\ScriptEngine\generated\");
            TestBinder.Init(@"test\");

            foreach (var filePath in Directory.GetFiles(fileDir))
            {
                var file = Path.GetFileName(filePath);
                if(file.EndsWith(".dll") && !IgnoreAssemblySet.Contains(file))
                {
                    TestBinder.TestBind(filePath);

                    if (file.StartsWith("UnityEngine."))
                        CBinder.Bind(filePath);
                    else
                        Binder.Bind(filePath);
                }
            }
            CBinder.End();
            Binder.End();
            TestBinder.End();

            Utils.Log("All Done..");
            Console.ReadLine();
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
