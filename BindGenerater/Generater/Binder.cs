using Generater.C;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
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
        static Dictionary<string, TypeReference> types = new Dictionary<string, TypeReference>();
        static HashSet<ModuleDefinition> moduleSet = new HashSet<ModuleDefinition>();
        static HashSet<TypeReference> moduleTypes;
        static HashSet<TypeReference> refTypes = new HashSet<TypeReference>();

        public static string OutDir;

        public static Dictionary<string, CSharpDecompiler> DecompilerDic = new Dictionary<string, CSharpDecompiler>();
        public static DecompilerSettings DecompilerSetting;
        public static string ManagedDir;
        public static ModuleDefinition curModule;

        private static string[] IgnorTypes = new string[]
        {
            "System.Collections",
            "UnityEditor",
            "UnityEngine.TestTools",

            //platform specific
            "UnityEngine.WSA",
            "UnityEngine.iOS",
            "UnityEngine.tvOS",
            "UnityEngine.Apple",
            "UnityEngine.Handheld",
            "UnityEngine.Social"
        };

        public static HashSet<string> UnityCoreModuleSet = new HashSet<string>
        {
            "UnityEngine.SharedInternalsModule.dll",
            "UnityEngine.CoreModule.dll"
        };

        public static HashSet<string> IgnoreUsing = new HashSet<string>()
        {
            "UnityEngine.Internal",
            "UnityEngine.Scripting.APIUpdating",
            "UnityEngine.Bindings",
            "Unity.IL2CPP.CompilerServices",
            "Unity.Burst",
            "UnityEngine.Bindings"
        };

        public static HashSet<string> retainTypes = new HashSet<string>()
        {
            "UnityEngine.Transform",
            "UnityEngine.Texture",
            "UnityEngine.Debug",
            "UnityEngine.ILogger",
            "UnityEngine.ILogHandler",
            "UnityEngine.Logger",
            "UnityEngine.DebugLogHandler",

            "UnityEngine.UnityException",
            "UnityEngine.UnityString",
            "UnityEngine.CastHelper`1",
            "UnityEngineInternal.MathfInternal"
        };

        public static void Init(string outDir)
        {
            OutDir = outDir;
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            Utils.IgnoreTypeSet.UnionWith(IgnorTypes);
        }

        public static void End()
        {
            TypeResolver.WrapperSide = false;
            GenerateBindings.End();

            foreach (var m in moduleSet)
                m.Dispose();

            moduleSet.Clear();
        }

        public static void Bind(string dllPath)
        {
            var file = Path.GetFileName(dllPath);

            DecompilerSetting = new DecompilerSettings(LanguageVersion.CSharp7);
            DecompilerSetting.ThrowOnAssemblyResolveErrors = false;
            DecompilerSetting.UseExpressionBodyForCalculatedGetterOnlyProperties = false;

            ManagedDir = Path.GetDirectoryName(dllPath);
            DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(ManagedDir);
            ReaderParameters parameters = new ReaderParameters()
            {
                AssemblyResolver = resolver,
                ReadSymbols = false,
            };

            curModule = ModuleDefinition.ReadModule(dllPath, parameters);
            moduleSet.Add(curModule);
            ICallGenerater.AddWrapperAssembly(curModule.Assembly.Name.Name);
            CSCGenerater.SetWrapper(file);
            GenerateBindings.StartWraper(file);
            CSCGenerater.AdapterCompiler.AddReference(curModule.Name);
            foreach(var refAssembly in curModule.AssemblyReferences )
            {
                CSCGenerater.AdapterCompiler.AddReference(refAssembly.Name + ".dll");
                CSCGenerater.AdapterWrapperCompiler.AddReference(refAssembly.Name + ".dll");
            }
            CSCGenerater.AdapterWrapperCompiler.RemoveReference(curModule.Name);

            moduleTypes = new HashSet<TypeReference>(curModule.Types);

            foreach (TypeDefinition type in moduleTypes)
            {
                if (!type.IsPublic || type.IsInterface)
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

            GenerateBindings.Gen();
        }

        public static void AddType(TypeDefinition type)
        {
            if (type == null || !moduleTypes.Contains(type))
                return;


            if (types.ContainsKey(type.FullName))
                return;
            types[type.FullName] = type;

            if (!Utils.Filter(type))
                return;

            generaters.Enqueue(new ClassGenerater(type));
            
        }

        public static void AddTypeRef(TypeReference type)
        {
            refTypes.Add(type);
            var td = type.Resolve();
            if (td == null)
                return;
            if (types.ContainsKey(td.FullName))
                return;
            types[td.FullName] = td;

            if (!Utils.Filter(type))
                return;

            generaters.Enqueue(new ClassGenerater(td));
        }

        public static CSharpDecompiler GetDecompiler(string module)
        {
            CSharpDecompiler decompiler = null;
            
            if (DecompilerDic.TryGetValue(module, out decompiler))
                return decompiler;

            var dllPath = Path.Combine(ManagedDir, module);
            decompiler = new CSharpDecompiler(dllPath, DecompilerSetting);
            DecompilerDic[module] = decompiler;
            return decompiler;
        }
        
    }
}