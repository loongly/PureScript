using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Generater.C
{
    class ICallGenerater
    {
        static CodeWriter IcallWriter;

        static ICallGenerater()
        {
            IcallWriter = new CodeWriter(File.CreateText(Path.Combine(CBinder.OutDir, "icall_binding_gen.c")));
        }

        static HashSet<MethodDefinition> methodSet = new HashSet<MethodDefinition>();
        static HashSet<string> wrapperAssemblySet = new HashSet<string>();
        public static void AddMethod(MethodDefinition method)
        {
            if(!CUtils.IsCustomICall(CUtils.GetICallDescName(method)))
                methodSet.Add(method);
        }

        public static void AddWrapperAssembly(string name)
        {
            wrapperAssemblySet.Add(name);
        }

        public static void Gen()
        {
            using (new CS(IcallWriter))
            {
                CS.Writer.WriteLine("#include \"engine_include.h\"", false);
                CS.Writer.WriteLine("#include \"class_cache_gen.h\"", false);

                RegisterAssemblyMap();

                foreach (var m in methodSet)
                {
                    CS.Writer.Start($"{CTypeResolver.Resolve(m.ReturnType).TypeName()} {CUtils.ImplementMethodName(m, true)}");
                    ImplementBindMethod(m);
                    CS.Writer.End();
                }

                CS.Writer.Start("void regist_icall_gen()");
                foreach (var m in methodSet)
                {
                    CS.Writer.WriteLine($"mono_add_internal_call(\"{CUtils.GetICallDescName(m)}\",(void*) {CUtils.ImplementMethodName(m, false)})");
                }
                CS.Writer.End();
            }

            IcallWriter.EndAll();
        }

        private static void ImplementBindMethod(MethodDefinition method)
        {
            var i2ReturnTypeName = CTypeResolver.Resolve(method.ReturnType, true).TypeName();

            CS.Writer.WriteLine($"typedef {i2ReturnTypeName} (* ICallMethod) {CUtils.GetParamDefine(method, true)}");
            CS.Writer.WriteLine("static ICallMethod icall");
            CS.Writer.WriteLine("if(!icall)",false);
            CS.Writer.WriteLine("\t"+$"icall = (ICallMethod)il2cpp_resolve_icall(\"{CUtils.GetICallDescName(method)}\")");

            if (method.ReturnType.IsVoid())
                CS.Writer.WriteLine("icall(", false);
            else
                CS.Writer.WriteLine($"{i2ReturnTypeName} i2res = icall(", false);

            if(!method.IsStatic)
            {
                CS.Writer.Write(CTypeResolver.Resolve(method.DeclaringType).Unbox("thiz", true));
                if (method.Parameters.Count > 0)
                    CS.Writer.Write(",");
            }

            var lastP = method.Parameters.LastOrDefault();
            foreach (var p in method.Parameters)
            {
                CS.Writer.Write(CTypeResolver.Resolve(p.ParameterType).Unbox(p.Name, true));
                if (lastP != p)
                    CS.Writer.Write(",");
            }
            CS.Writer.Write(");");

            if (!method.ReturnType.IsVoid())
            {
                var monoRes = CTypeResolver.Resolve(method.ReturnType).Box("i2res");
                CS.Writer.WriteLine($"return {monoRes}");
            }
        }

        /*
         void register_assembly_map()
            {
                insert_assembly_map("AdapterTest", "Adapter.wrapper");
            }
            */
        private static void RegisterAssemblyMap()
        {
            CS.Writer.Start("void register_assembly_map()");
            /*foreach(var assembly in wrapperAssemblySet)
                CS.Writer.WriteLine($"insert_assembly_map(\"{assembly}\", \"Adapter.wrapper\")");*/
            CS.Writer.End();
        }
    }
}
