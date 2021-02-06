using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generater
{
    public static class Binder
    {
        static Queue<CodeGenerater> generaters = new Queue<CodeGenerater>();
        static HashSet<TypeReference> types = new HashSet<TypeReference>();
        static HashSet<TypeReference> moduleTypes;

        public static string OutDir;
        public static CodeWriter FuncDefineWriter;
        public static CodeWriter FuncSerWriter;
        public static CodeWriter FuncDeSerWriter;
        public static CSharpDecompiler Decompiler;
        public static DecompilerSettings DecompilerSetting;


        private static string[] IgnorTypes = new string[]
        {
           // "Unity.Collections.NativeArray`1<T>",
            "UnityEngine.WSA",
           // "Unity.Collections.LowLevel.Unsafe",
            "System.Collections",
            "UnityEditor",

            "AtomicSafetyHandle", // cant used in script
            "TransformAccessArray",//rely on above

            "UnityEngine.Experimental.GlobalIllumination.Lightmapping", //NativeArray`1<T>

            //Windows Strip
            "UnityEngine.iOS",
            "UnityEngine.tvOS",
            "UnityEngine.Apple",
            "UnityEngine.Handheld"
        };

        public static void Init(string outDir)
        {
            OutDir = outDir;
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            FuncDefineWriter = new CodeWriter(File.CreateText(Path.Combine(outDir, "Binder.define.cs")));
            FuncSerWriter = new CodeWriter(File.CreateText(Path.Combine(outDir, "Binder.funcser.cs")));
            FuncDeSerWriter = new CodeWriter(File.CreateText(Path.Combine(outDir, "Binder.funcdeser.cs")));
        }

        public static void End()
        {

        }

        public static void Bind(string dllPath)
        {
            DecompilerSetting = new DecompilerSettings(LanguageVersion.CSharp7);
            Decompiler = new CSharpDecompiler(dllPath, DecompilerSetting);
            var dir = Path.GetDirectoryName(dllPath);
            DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(dir);
            ReaderParameters parameters = new ReaderParameters()
            {
                AssemblyResolver = resolver,
                ReadSymbols = false,
            };

            ModuleDefinition module = ModuleDefinition.ReadModule(dllPath, parameters);

            moduleTypes = new HashSet<TypeReference>(module.Types);
            var ignorSet = Utils.IgnoreTypeSet;
            foreach(var type in IgnorTypes)
            {
                ignorSet.Add(type);
            }

            //var typeSet = new HashSet<string>(BindTypes);

            foreach (TypeDefinition type in moduleTypes)
            {
                if (!type.IsPublic)
                    continue;

                AddType(type);

               /* Console.WriteLine(type.FullName);
                if (typeSet.Contains(type.Name))
                {
                    AddType(type);
                }*/
            }

            TypeResolver.WrapperSide = true;

            var gener = generaters.Dequeue();
            while (gener != null)
            {
                gener.Gen();

                if (generaters.Count < 1)
                    break;
                else
                    gener = generaters.Dequeue();
            }
            
            TypeResolver.WrapperSide = false;
            GenerateBindings.Gen();
            FuncDefineWriter.EndAll();
            FuncSerWriter.EndAll();
            FuncDeSerWriter.EndAll();

        }

        public static void AddType(TypeDefinition type)
        {
            if (type == null || !moduleTypes.Contains(type))
                return;
            if (!types.Add(type))
                return;

            if (!Utils.Filter(type))
                return;

            generaters.Enqueue(new ClassGenerater(type));

            /*CodeGenerater gener = null;
            if (type.IsValueType || type.IsEnum || type.IsDelegate())
            {
                CopyGenerater.AddTpe(type);
            }
            else if (type.IsClass)
            {
                var baseType = type.BaseType;
                var bt = baseType.Resolve();
                if (baseType.Resolve().IsAbstract)
                    baseType = bt.BaseType;
                while (baseType != null && baseType.FullName != "System.Object")
                {
                    bt = baseType.Resolve();
                    AddType(bt);
                    baseType = bt.BaseType;
                }

                gener = new ClassGenerater(type);
            }
            else if(type.IsInterface)
            {
                gener = new ClassGenerater(type);
            }
            
            if(gener != null)
                generaters.Enqueue(gener);*/
        }
    }
}