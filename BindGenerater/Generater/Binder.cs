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
        static Dictionary<TypeDefinition, CodeGenerater> TypeDic = new Dictionary<TypeDefinition, CodeGenerater>();

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

            var ignorSet = Utils.IgnoreTypeSet;
            foreach(var type in IgnorTypes)
            {
                ignorSet.Add(type);
            }

            var typeSet = new HashSet<string>(BindTypes);
            foreach (TypeDefinition type in module.Types)
            {
                if (!type.IsPublic)
                    continue;

                Console.WriteLine(type.FullName);

                if (typeSet.Contains(type.Name))
                {
                    AddType(type);
                }
            }

            foreach (var gener in TypeDic)
            {
                var filePath = Path.Combine(outDir, $"Binder.{gener.Key.FullName}.cs");
                using (new CS(new CodeWriter(File.CreateText(filePath))))
                {
                    gener.Value.Gen();
                }
            }

            GenerateBindings.Gen();
            FuncDefineWriter.EndAll();
            FuncSerWriter.EndAll();
            FuncDeSerWriter.EndAll();
        }

        public static void AddType(TypeDefinition type)
        {
            if (TypeDic.ContainsKey(type))
                return;

            var classGen = new ClassGenerater(type);
            TypeDic[type] = classGen;
        }





    }
}