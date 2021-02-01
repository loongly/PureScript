using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;

namespace Generater
{
    public static class GenerateBindings
    {
        static List<MethodDefinition> methods = new List<MethodDefinition>();
        public static void AddMethod(MethodDefinition method)
        {
            methods.Add(method);
        }

        public static void Gen()
        {
            using (new CS(Binder.FuncDefineWriter))
            {
                foreach(var method in methods)
                {
                    CS.Writer.WriteLine("[UnmanagedFunctionPointer(CallingConvention.Cdecl)]", false);
                    CS.Writer.WriteLine($"public delegate {BindResolver.Resolve(method.ReturnType).TypeName()} {Utils.BindMethodName(method,true,false)}_Type {Utils.GetParamDefine(method,true)}");
                }
            }

            using (new CS(Binder.FuncSerWriter))
            {
                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false);
                    CS.Writer.WriteLine($"static readonly {methodName}_Type {methodName}Delegate = new {methodName}_Type({methodName})");
                }

                CS.Writer.Start("public static void BindFunc()");
                CS.Writer.WriteLine("IntPtr memory = Marshal.AllocHGlobal(1024)");
                CS.Writer.WriteLine("int curMemory = 0;");
                
                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false) + "Delegate";
                    CS.Writer.WriteLine($"Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate({methodName}))");
                    CS.Writer.WriteLine("curMemory += IntPtr.Size");
                }

                CS.Writer.WriteLine("MonoLib.SetFuncPointer(memory)");
                CS.Writer.End();

                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false);
                    CS.Writer.WriteLine($"[MonoPInvokeCallback(typeof({methodName}DelegateType))]", false);
                    CS.Writer.Start($"static {BindResolver.Resolve(method.ReturnType).TypeName()} {methodName} {Utils.GetParamDefine(method, true)}");
                    var reName = BindResolver.Resolve(method.ReturnType).Implement(method,"value");
                    CS.Writer.WriteLine($"return {reName}");
                    CS.Writer.End();
                }
            }

            using (new CS(Binder.FuncDeSerWriter))
            {
                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false);
                    CS.Writer.WriteLine($"public static {methodName}_Type {methodName}");
                }

                CS.Writer.Start("public static void InitBind(IntPtr memory)");
                CS.Writer.Start("if(memory == IntPtr.Zero)");
                CS.Writer.WriteLine("memory = GetManageFuncPtr()");
                CS.Writer.End();
                CS.Writer.WriteLine("int curMemory = 0");

                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false);
                    CS.Writer.WriteLine($"{methodName} = Marshal.GetDelegateForFunctionPointer<{methodName}_Type>(Marshal.ReadIntPtr(memory, curMemory))");
                    CS.Writer.WriteLine("curMemory += IntPtr.Size");
                }

                CS.Writer.End();
            }

        }


    }
}