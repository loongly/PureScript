using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;

namespace Generater
{
    public static class GenerateBindings
    {
        static HashSet<MethodDefinition> methods = new HashSet<MethodDefinition>();
        static HashSet<string> delegateDefines = new HashSet<string>();
        public static void AddMethod(MethodDefinition method)
        {
            methods.Add(method);
        }

        public static void AddDelegateDefine(string defineStr)
        {
            delegateDefines.Add(defineStr);
        }

        public static void Gen()
        {
            var nsSet = Utils.GetNameSpaceList(methods);
            nsSet.Add("System.Runtime.InteropServices");
            nsSet.Add("System.Collections.Generic");
            nsSet.Add("Object = UnityEngine.Object");
            nsSet.Add("Random = UnityEngine.Random");


            using (new CS(Binder.FuncDefineWriter))
            {
                foreach(var ns in nsSet)
                    CS.Writer.WriteLine($"using {ns}");

                foreach (var method in methods)
                {
                    CS.Writer.WriteLine("[UnmanagedFunctionPointer(CallingConvention.Cdecl)]", false);
                    //MethodResolver.Resolve(method).DefineDelegate();
                    var flag = Utils.IsUnsafeMethod(method) ? " unsafe " : " ";
                    CS.Writer.WriteLine($"public{flag}delegate {MethodResolver.Resolve(method).ReturnType()} {Utils.BindMethodName(method,true,false)}_Type {Utils.BindMethodParamDefine(method,true)}");
                }

                foreach(var define in delegateDefines)
                {
                    CS.Writer.WriteLine(define);
                }
            }

            using (new CS(Binder.FuncDeSerWriter))
            {
                foreach (var ns in nsSet)
                    CS.Writer.WriteLine($"using {ns}");

                CS.Writer.Start("public static class MonoBind");

                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false);
                    CS.Writer.WriteLine($"public static {methodName}_Type {methodName}");
                }

                CS.Writer.Start("public static void InitBind(IntPtr memory)");
                //CS.Writer.Start("if(memory == IntPtr.Zero)");
                //CS.Writer.WriteLine("memory = GetManageFuncPtr()");
                //CS.Writer.End();
                CS.Writer.WriteLine("int curMemory = 0");

                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false);
                    CS.Writer.WriteLine($"{methodName} = Marshal.GetDelegateForFunctionPointer<{methodName}_Type>(Marshal.ReadIntPtr(memory, curMemory))");
                    CS.Writer.WriteLine("curMemory += IntPtr.Size");
                }

                CS.Writer.WriteLine("Custom.DeSer(memory + curMemory)");

                CS.Writer.EndAll();
            }

            using (new CS(Binder.FuncSerWriter))
            {
                foreach (var ns in nsSet)
                    CS.Writer.WriteLine($"using {ns}");

                CS.Writer.WriteLine($"using AOT");

                CS.Writer.Start("public static unsafe class UnityBind");

                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false);
                    CS.Writer.WriteLine($"static readonly {methodName}_Type {methodName}Delegate = new {methodName}_Type({methodName})");
                }

                CS.Writer.Start("public static IntPtr BindFunc()");
                CS.Writer.WriteLine("IntPtr memory = Marshal.AllocHGlobal(8192*8)");
                CS.Writer.WriteLine("int curMemory = 0;");
                
                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false) + "Delegate";
                    CS.Writer.WriteLine($"Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate({methodName}))");
                    CS.Writer.WriteLine("curMemory += IntPtr.Size");
                }

                CS.Writer.WriteLine("Custom.Ser(memory + curMemory)");
                CS.Writer.WriteLine("return memory");
                CS.Writer.End();

                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false);


                    CS.Writer.CreateLinePoint("//Method");

                    CS.Writer.WriteLine($"[MonoPInvokeCallback(typeof({methodName}_Type))]", false);
                    CS.Writer.Start($"static {MethodResolver.Resolve(method).ReturnType()} {methodName} {Utils.BindMethodParamDefine(method, true)}");

                    CS.Writer.WriteLine("Exception __e = null");
                    CS.Writer.Start("try");

                    var reName = MethodResolver.Resolve(method).Implement("_value");
                    if (!string.IsNullOrEmpty(reName))
                        CS.Writer.WriteLine($"return {reName}");
                    CS.Writer.End();//try
                    CS.Writer.Start("catch(Exception e)");
                    CS.Writer.WriteLine("__e = e");
                    CS.Writer.End();//catch

                    CS.Writer.WriteLine("if(__e != null)", false);
                    CS.Writer.WriteLine("ScriptEngine.OnException(__e.ToString())");
                    if (!string.IsNullOrEmpty(reName))
                        CS.Writer.WriteLine($"return default({MethodResolver.Resolve(method).ReturnType()})");

                    CS.Writer.End();//method

                }

                CS.Writer.End();
            }

        }
    }
}