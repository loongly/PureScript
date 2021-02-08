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
        static void Main(string[] args)
        {
            Console.WriteLine("Hello world.");

            string fileDir = @"..\..\EngineLib\full\";
            Binder.Init(@"..\..\glue\");
            foreach (var filePath in Directory.GetFiles(fileDir))
            {
                var file = Path.GetFileName(filePath);
                if (file.StartsWith("UnityEngine.") && file.EndsWith(".dll"))
                if(file != "UnityEngine.UI.dll")
				//if(file == "UnityEngine.CoreModule.dll")
                {
                    Binder.Bind(filePath);
                }
            }
            Binder.End();

        }



        static void TestWriter()
        {
            using (new CS(new CodeWriter(File.CreateText("test.txt"))))
            {
                CS.Writer.WriteLine("1");
                CS.Writer.WriteLine("2");
                CS.Writer.WriteLine("3");
                CS.Writer.CreateLinePoint("//aa");

                CS.Writer.WriteLine("4");
                CS.Writer.WriteLine("5");

                using (new LP(CS.Writer.GetLinePoint("//aa")))
                {
                    CS.Writer.WriteLine("aa 1");
                    CS.Writer.WriteLine("aa 2");
                }

                CS.Writer.WriteLine("6");
            }
        }
    }
}
