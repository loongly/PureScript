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

        private static string[] BindTypes = new string[]
        {
            "GameObject","Transform","Debug",
            "Application","RectTransform","Input",
            "Resources","Time","AssetBundle",
            "Camera","Material","Texture2D"
        };
        private static string[] IgnorTypes = new string[]
        {
            "Unity.Collections.NativeArray`1<T>",
        };


        public static void Bind(string dllPath, string outDir)
        {
            OutDir = outDir;
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            FuncDefineWriter = new CodeWriter(File.CreateText(Path.Combine(outDir, "Binder.define.cs")));
            FuncSerWriter = new CodeWriter(File.CreateText(Path.Combine(outDir, "Binder.funcser.cs")));
            FuncDeSerWriter = new CodeWriter(File.CreateText(Path.Combine(outDir, "Binder.funcdeser.cs")));

            ModuleDefinition module = ModuleDefinition.ReadModule(dllPath);
            moduleTypes = new HashSet<TypeReference>(module.Types);
            var ignorSet = Utils.IgnoreTypeSet;
            foreach(var type in IgnorTypes)
            {
                ignorSet.Add(type);
            }

            var typeSet = new HashSet<string>(BindTypes);

            foreach (TypeDefinition type in moduleTypes)
            {
                if (!type.IsPublic)
                    continue;

                Console.WriteLine(type.FullName);

                if (typeSet.Contains(type.Name))
                {
                    AddType(type);
                }
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

            CopyGenerater.GenAsm();
        }

        public static void AddType(TypeDefinition type)
        {
            if (type.Name == "UnityAction")
                Utils.Log("");

            if (type == null || !moduleTypes.Contains(type))
                return;
            if (!types.Add(type))
                return;

            CodeGenerater gener = null;
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
                generaters.Enqueue(gener);
        }
    }
}