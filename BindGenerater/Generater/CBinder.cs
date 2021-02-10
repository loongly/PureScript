using Generater.C;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;

namespace Generater
{
    public class CBinder
    {
        public static string OutDir;
        private static HashSet<TypeReference> TypeSet = new HashSet<TypeReference>();

        public static void Init(string outDir)
        {
            OutDir = outDir;
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            CUtils.IgnoreTypeSet.Add("System.Type");
            CUtils.IgnoreTypeSet.Add("UnityEngine.StackTraceUtility");
        }

        public static void End()
        {
            EventGenerater.Gen();
            ICallGenerater.Gen();
            ClassCacheGenerater.Gen();
        }

        public static void AddType(TypeReference type)
        {
            TypeSet.Add(type);
        }

        public static void Bind()
        {
            foreach (TypeDefinition type in TypeSet)
            {
                //Console.WriteLine(type.FullName);
                if(!CUtils.Filter(type))
                {
                    Console.WriteLine("ignor type:" + type.FullName);
                    continue;
                }

                foreach (var method in type.Methods)
                {
                    if (CUtils.IsIcall(method))
                    {
                        if (!CUtils.Filter(method))
                        {
                            Console.WriteLine("ignor icall:"+ method.FullName);
                            continue;
                        }

                        ICallGenerater.AddMethod(method);
                    }
                    else if (CUtils.IsEventCallback(method) && !method.IsConstructor)
                    {
                        if (!CUtils.Filter(method))
                        {
                            Console.WriteLine("ignor event:" + method.FullName);
                            continue;
                        }

                        EventGenerater.AddMethod(method);
                    }
                }
            }
        }

        public static void TestType(string dllPath)
        {
            var dir = Path.GetDirectoryName(dllPath);
            DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(dir);
            ReaderParameters parameters = new ReaderParameters()
            {
                AssemblyResolver = resolver,
                ReadSymbols = false,
            };

            ModuleDefinition module = ModuleDefinition.ReadModule(dllPath, parameters);
            var moduleTypes = new HashSet<TypeReference>(module.Types);

            foreach (TypeDefinition type in moduleTypes)
            {
                if(Utils.IsUnityICallBind(type))
                    Console.WriteLine($"{type.FullName}");
            }
        }

    }
}
