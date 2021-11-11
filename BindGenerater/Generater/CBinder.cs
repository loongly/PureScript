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
        static HashSet<ModuleDefinition> moduleSet = new HashSet<ModuleDefinition>();

        public static void Init(string outDir)
        {
            OutDir = outDir;
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            CUtils.IgnoreTypeSet = new HashSet<string>(Config.Instance.ICallIgnorTypes);
        }

        public static void End()
        {
            EventGenerater.Gen();
            ICallGenerater.Gen();
            ClassCacheGenerater.Gen();

            foreach (var m in moduleSet)
                m.Dispose();

            moduleSet.Clear();
        }

        public static void Bind(string dllPath)
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
            moduleSet.Add(module);

            var moduleTypes = new HashSet<TypeReference>(module.Types);

            foreach (TypeDefinition type in moduleTypes)
            {
                //Utils.Log(type.FullName);

                foreach (var method in type.Methods)
                {
                    if (CUtils.IsIcall(method))
                    {
                        if (!CUtils.Filter(method))
                        {
                            Utils.Log("ignor icall:"+ method.FullName);
                            continue;
                        }

                        ICallGenerater.AddMethod(method);
                    }
                    else if (CUtils.IsEventCallback(method) && !method.IsConstructor)
                    {
                        if (!CUtils.Filter(method))
                        {
                            Utils.Log("ignor event:" + method.FullName);
                            continue;
                        }

                        EventGenerater.AddMethod(method);
                    }
                }
            }
        }

    }
}
