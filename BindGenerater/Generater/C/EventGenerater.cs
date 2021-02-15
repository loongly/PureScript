using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Generater.C
{
    public static class EventGenerater
    {
        static CodeWriter EventWriter;
        static HashSet<MethodDefinition> methodSet = new HashSet<MethodDefinition>();

        static EventGenerater()
        {
            EventWriter = new CodeWriter(File.CreateText(Path.Combine(CBinder.OutDir, "event_binding_gen.c")));
        }

        public static void AddMethod(MethodDefinition method)
        {
            methodSet.Add(method);
        }

        public static void Gen()
        {
            using (new CS(EventWriter))
            {
                CS.Writer.WriteLine("#include \"../custom/event_binding.h\"", false);
                CS.Writer.WriteLine("#include \"class_cache_gen.h\"", false);

                CS.Writer.WriteLine($"EventMethodDesc methods[{methodSet.Count}]");

                int index = 0;
                foreach (var m in methodSet)
                {
                    CS.Writer.Start($"{CTypeResolver.Resolve(m.ReturnType,true).TypeName()} {CUtils.ImplementMethodName(m, false) + CUtils.GetParamDefine(m, true, "const MethodInfo* imethod")} ");
                    ImplementEventMethod(m,index);
                    CS.Writer.End();
                    index++;
                }

                index = 0;
                CS.Writer.Start("void init_event_gen()");
                foreach (var m in methodSet)
                {
                    CS.Writer.WriteLine($"init_event_method(&methods[{index}],{ClassCacheGenerater.GetClass(m.DeclaringType,false)},{ClassCacheGenerater.GetClass(m.DeclaringType, true)}," +
                        $"\"{m.Name}\",{m.Parameters.Count},(Il2CppMethodPointer) {CUtils.ImplementMethodName(m, false)})");
                    index++;
                }
                CS.Writer.End();
            }

            EventWriter.EndAll();
        }

        /*
        void UnityEngine_AsyncOperation_InvokeCompletionEvent(Il2CppObject* obj, const MethodInfo* imethod)
        {
	        const int index = 52;
	        typedef void(*THUNK_METHOD EventMethod) (MonoObject* obj, MonoException** exc);
	        static EventMethod thunk;
	        if (!thunk)
		        thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	        MonoException *exc = NULL;
	        MonoObject* monoobj = get_mono_object(obj, mono_get_class_UnityEngine_AsyncOperation());
	        thunk(monoobj, &exc);
        }
             */
        private static void ImplementEventMethod(MethodDefinition method,int index)
        {
            var returnTypeName = CTypeResolver.Resolve(method.ReturnType, false).TypeName();

            CS.Writer.WriteLine($"const int index = {index}");
            CS.Writer.WriteLine($"typedef {returnTypeName} (* THUNK_METHOD EventMethod) {CUtils.GetParamDefine(method, false, "MonoException** exc")}");
            CS.Writer.WriteLine("static EventMethod thunk");
            CS.Writer.WriteLine("if(!thunk)",false);
            CS.Writer.WriteLine("\t" + $"thunk = mono_method_get_unmanaged_thunk(methods[index].hooked)");

            CS.Writer.WriteLine("MonoException *exc = NULL");

            if (method.ReturnType.IsVoid())
                CS.Writer.WriteLine("thunk(", false);
            else
                CS.Writer.WriteLine($"{returnTypeName} res = thunk(", false);

            if (!method.IsStatic)
            {
                CS.Writer.Write(CTypeResolver.Resolve(method.DeclaringType).Box("thiz",true));
                //if (method.Parameters.Count > 0)
                CS.Writer.Write(",");
            }

            var lastP = method.Parameters.LastOrDefault();
            foreach (var p in method.Parameters)
            {
                CS.Writer.Write(CTypeResolver.Resolve(p.ParameterType).Box(p.Name, true));
                //if (lastP != p)
                CS.Writer.Write(",");
            }

            CS.Writer.Write("&exc);");

            if (!method.ReturnType.IsVoid())
            {
                var monoRes = CTypeResolver.Resolve(method.ReturnType).Unbox("res",false);
                CS.Writer.WriteLine($"return {monoRes}");
            }
        }

    }
}
