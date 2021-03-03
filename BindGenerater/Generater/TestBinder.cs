using Generater;
using Generater.C;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;

namespace Generater
{
    public class TestBinder
    {
        const bool EnableTest = false;
        public static string OutDir;
        static int index = 0;

        static StreamWriter sw ;

        public static void Init(string outDir)
        {
            if (!EnableTest)
                return;

            OutDir = outDir;
            sw = File.CreateText(outDir + "unityObject.cs");

            string[] ignore = new string[]
            {
                "System.Type",
                "System.Reflection",
                "Unity.Burst.LowLevel",
                "UnityEngine.StackTraceUtility",
                "Audio.AudioSampleProvider",
                "BuiltinRuntimeReflectionSystem",
                "ScriptableRuntimeReflectionSystemWrapper",
                "IScriptableRuntimeReflectionSystem",
                "SubsystemDescriptor",
                "System.Collections.IDictionary",
                "UnityEngine.SocialPlatforms",
                "UnityEngine.iOS.LocalNotification",
                "UnityEngine.AttributeHelperEngine",
                "UnityEngine.ComputeBuffer"
                //UnsafeUtility?
            };

            CUtils.IgnoreTypeSet = new HashSet<string>(ignore);
        }

        public static void TestBind(string dllPath)
        {
            if (!EnableTest)
                return;

            sw.WriteLine("// >>>> " + Path.GetFileName(dllPath));
            checkSet.Clear();

            var dir = Path.GetDirectoryName(dllPath);
            DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(dir);
            ReaderParameters parameters = new ReaderParameters()
            {
                AssemblyResolver = resolver,
                ReadSymbols = false,
            };

            ModuleDefinition module = ModuleDefinition.ReadModule(dllPath, parameters);
            var moduleTypes = new HashSet<TypeDefinition>(module.Types);

            foreach (TypeDefinition type in moduleTypes)
            {
                if (!CUtils.Filter(type))
                {
                   // Console.WriteLine("ignor type:" + type.FullName);
                    continue;
                }

                foreach (var method in type.Methods)
                {
                    if (CUtils.IsIcall(method))
                    {
                        if (!CUtils.Filter(method))
                        {
                            //Console.WriteLine("ignor icall:" + method.FullName);
                            //checkSet.Add(method.FullName + " // <Icall");
                            continue;
                        }
                        CheckMethod(method);
                        //ICallGenerater.AddMethod(method);
                    }
                    //else if (CUtils.IsEventCallback(method) && !method.IsConstructor)
                    else if (CUtils.IsNativeCallback(method) && !method.IsConstructor)
                    {
                        if (!CUtils.Filter(method))
                        {
                            // Console.WriteLine("ignor event:" + method.FullName);
                           // checkSet.Add(method.FullName + " // <Event");
                            continue;
                        }
                        CheckMethod(method);
                        // EventGenerater.AddMethod(method);
                    }
                }
            }

            
            foreach (var c in checkSet)
            {
                sw.WriteLine($"public {c} T_{index};");
                index++;
            }
        }

        public static void End()
        {
            if (!EnableTest)
                return;

            sw?.Close();
        }


        static HashSet<string> checkSet = new HashSet<string>();
        public static void CheckMethod(MethodDefinition method)
        {
            bool issue = false;
            issue |= CheckType(method.ReturnType);
            foreach (var p in method.Parameters)
            {
                issue |= CheckType(p.ParameterType);
            }

            if (issue)
                checkSet.Add(method.FullName + " // <===");
        }

        static bool CheckType(TypeReference t)
        {
            return CheckHaveHeadPtr(t);
            //return CheckEvent(t);
           // return CheckManageStruct(t);
        }

        static bool CheckEvent(TypeReference t)
        {
            bool res = Utils.IsDelegate(t);
           // if (res)
           //     checkSet.Add(t.FullName);

            return res;
        }

        static bool CheckHaveHeadPtr(TypeReference t)
        {
            var res = !Utils.HaveHeadPtr(t);
           // if (res)
           //     checkSet.Add(t.FullName);

            return res;
        }

        static bool CheckManageStruct(TypeReference t)
        {
            var res = t.IsValueType && !Utils.IsFullValueType(t);
           // if (res)
           //     checkSet.Add(t.FullName);

            return res;
        }
    }
}
