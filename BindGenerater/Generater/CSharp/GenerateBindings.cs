using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Generater
{
    public class BindingGenerater
    {
        public string Name;
        public int Offset;
        HashSet<MethodDefinition> methods = new HashSet<MethodDefinition>();
        HashSet<string> delegateDefines = new HashSet<string>();

        StreamWriter Writer;

        public BindingGenerater(string name,int _offset,StreamWriter writer)
        {
            Name = name;
            Offset = _offset;
            Writer = writer;
        }

        public void AddMethod(MethodDefinition method)
        {
            methods.Add(method);
        }

        public void AddDelegateDefine(string defineStr)
        {
            delegateDefines.Add(defineStr);
        }

        private void GenDefines()
        {
            // method define
            foreach (var method in methods)
            {
                CS.Writer.WriteLine("[UnmanagedFunctionPointer(CallingConvention.Cdecl)]", false);
                //MethodResolver.Resolve(method).DefineDelegate();
                var flag = Utils.IsUnsafeMethod(method) ? " unsafe " : " ";
                CS.Writer.WriteLine($"public{flag}delegate {MethodResolver.Resolve(method).ReturnType()} {Utils.BindMethodName(method, true, false)}_Type {Utils.BindMethodParamDefine(method, true)}");
            }

            // delegate define
            foreach (var define in delegateDefines)
            {
                CS.Writer.WriteLine(define);
            }
        }

        public void GenWrapper()
        {
            var nsSet = Utils.GetNameSpaceList(methods);
            if(methods.Count > 0)
            {
                nsSet.Add("System.Runtime.InteropServices");
                nsSet.Add("System.Collections.Generic");
                nsSet.Add("Object = UnityEngine.Object");
                nsSet.Add("Random = UnityEngine.Random");
            }
            nsSet.Add("System");

            using (new CS(new CodeWriter(Writer)))
            {

                foreach (var ns in nsSet)
                    CS.Writer.WriteLine($"using {ns}");

                GenDefines();

                // wrapper imple
                //CS.Writer.WriteLine("using PureScript.Mono");

                CS.Writer.Start("internal static partial class MonoBind");

                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false);
                    CS.Writer.WriteLine($"public static {methodName}_Type {methodName}");
                }

                CS.Writer.Start("static MonoBind()");
                CS.Writer.WriteLine("InitBind(PureScript.Mono.ScriptEngine.GetFuncPointer())");
                CS.Writer.End();

                CS.Writer.Start("public static void InitBind(IntPtr memory)");
                //CS.Writer.Start("if(memory == IntPtr.Zero)");
                //CS.Writer.WriteLine("memory = GetManageFuncPtr()");
                //CS.Writer.End();

                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false);
                    CS.Writer.WriteLine($"{methodName} = Marshal.GetDelegateForFunctionPointer<{methodName}_Type>(Marshal.ReadIntPtr(memory, {Offset} * IntPtr.Size ))");
                    Offset++;
                }

                CS.Writer.EndAll();
            }
        }

        public void GenImpl()
        {
            var nsSet = Utils.GetNameSpaceList(methods);
            nsSet.Add("System.Runtime.InteropServices");
            nsSet.Add("System.Collections.Generic");
            nsSet.Add("Object = UnityEngine.Object");
            nsSet.Add("Random = UnityEngine.Random");
            nsSet.Add("PureScript");
            nsSet.Add("AOT");
            nsSet.Add("System");


            using (new CS(new CodeWriter(Writer)))
            {
                foreach (var ns in nsSet)
                    CS.Writer.WriteLine($"using {ns}");

                CS.Writer.Start("public static unsafe class UnityBind");

                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false);
                    CS.Writer.WriteLine($"static readonly {methodName}_Type {methodName}Delegate = new {methodName}_Type({methodName})");
                }

                CS.Writer.Start("public static IntPtr BindFunc()");
                CS.Writer.WriteLine($"IntPtr memory = Marshal.AllocHGlobal({methods.Count + 1} * IntPtr.Size)");

                foreach (var method in methods)
                {
                    var methodName = Utils.BindMethodName(method, true, false) + "Delegate";
                    CS.Writer.WriteLine($"Marshal.WriteIntPtr(memory, {Offset} * IntPtr.Size, Marshal.GetFunctionPointerForDelegate({methodName}))");
                    Offset++;
                }

                CS.Writer.WriteLine($"Custom.Ser(memory + {Offset} * IntPtr.Size)");
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

                CS.Writer.End();//UnityBind

                GenDefines();

                CS.Writer.EndAll();
            }
        }
    }

    public static class GenerateBindings
    {
        static BindingGenerater implGenerater;
        static BindingGenerater wrapGenerater;

        public static void StartWraper(string file)
        {
            if (implGenerater == null)
            {
                var implName = "Binder.impl.cs";
                var implWriter = File.CreateText(Path.Combine(Binder.OutDir, implName));
                implGenerater = new BindingGenerater("Binder.impl",0, implWriter);
            }

            var name = $"Binder.{file.Replace(".dll",".cs")}";
            var path = Path.Combine(Binder.OutDir, name);
            var writer = File.CreateText(path);
            var offset = wrapGenerater != null ? wrapGenerater.Offset : 0;
            wrapGenerater = new BindingGenerater(name, offset, writer);

            CSCGenerater.AdapterWrapperCompiler.AddSource(path);
        }

        public static void AddMethod(MethodDefinition method)
        {
            implGenerater.AddMethod(method);
            wrapGenerater.AddMethod(method);
        }

        public static void AddDelegateDefine(string defineStr)
        {
            implGenerater.AddDelegateDefine(defineStr);
            wrapGenerater.AddDelegateDefine(defineStr);
        }

        public static void Gen()
        {
            wrapGenerater.GenWrapper();
        }
        public static void End()
        {
            implGenerater.GenImpl();
        }
    }
}