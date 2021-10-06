using Generater.C;
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
        static HashSet<ModuleDefinition> moduleSet = new HashSet<ModuleDefinition>();
        static HashSet<TypeReference> moduleTypes;

        public static string OutDir;
        public static CodeWriter FuncDefineWriter;
        public static CodeWriter FuncSerWriter;
        public static CodeWriter FuncDeSerWriter;
        public static CSharpDecompiler Decompiler;
        public static DecompilerSettings DecompilerSetting;

        private static string[] IgnorTypes = new string[]
        {
            "System.Collections",
            "UnityEditor",
        };

        public static void Init(string outDir)
        {
            OutDir = outDir;
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            FuncDefineWriter = new CodeWriter(File.CreateText(Path.Combine(outDir, "Binder.define.cs")));
            FuncSerWriter = new CodeWriter(File.CreateText(Path.Combine(outDir, "Binder.funcser.cs")));
            FuncDeSerWriter = new CodeWriter(File.CreateText(Path.Combine(outDir, "Binder.funcdeser.cs")));

            Utils.IgnoreTypeSet.UnionWith(IgnorTypes);
        }

        public static void End()
        {
            TypeResolver.WrapperSide = false;
            GenerateBindings.Gen();
            FuncDefineWriter.EndAll();
            FuncSerWriter.EndAll();
            FuncDeSerWriter.EndAll();

            foreach (var m in moduleSet)
                m.Dispose();

            moduleSet.Clear();
        }

        public static void Bind(string dllPath)
        {
            /*var file = Path.GetFileName(dllPath);
            if (IgnoreAssemblySet.Contains(file))
                return;*/
            var file = Path.GetFileName(dllPath);

            DecompilerSetting = new DecompilerSettings(LanguageVersion.CSharp7);
            DecompilerSetting.ThrowOnAssemblyResolveErrors = false;
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
            moduleSet.Add(module);
            ICallGenerater.AddWrapperAssembly(module.Assembly.Name.Name);
            CSCGenerater.SetWrapper(file);
            CSCGenerater.AdapterCompiler.AddReference(module.Name);
            foreach(var refAssembly in module.AssemblyReferences )
            {
                CSCGenerater.AdapterCompiler.AddReference(refAssembly.Name + ".dll");
                CSCGenerater.AdapterWrapperCompiler.AddReference(refAssembly.Name + ".dll");
            }

            moduleTypes = new HashSet<TypeReference>(module.Types);

            foreach (TypeDefinition type in moduleTypes)
            {
                if (!type.IsPublic)
                    continue;

                AddType(type);
            }

            TypeResolver.WrapperSide = true;

            while(generaters.Count > 0)
            {
                var gener = generaters.Dequeue();
                if (gener != null)
                    gener.Gen();
            }
            generaters.Clear();
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
            
        }
    }
}