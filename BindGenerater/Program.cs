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
                if(file == "UnityEngine.CoreModule.dll")
                {
                        Binder.Bind(filePath);
                    }
            }
            Binder.End();

        }
    }
}
