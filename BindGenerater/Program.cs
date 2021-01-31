using Generater;
using Mono.Cecil;
using System;
using System.Collections.Generic;
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

            string fileName = "UnityEngine.CoreModule.dll";
            Binder.Bind(fileName, "gen");
           
        }
    }
}
