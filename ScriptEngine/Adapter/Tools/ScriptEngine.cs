using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System;
using System.Reflection;

namespace PureScript.Mono
{
    public class ScriptEngine
    {

        const string XMONO_LIB = "ScriptEngine";

        [DllImport(XMONO_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetFuncPointer();

        [DllImport(XMONO_LIB, EntryPoint = "OnExceptionMono", CallingConvention = CallingConvention.Cdecl)]
        public static extern void OnException(string msg);
        [DllImport(XMONO_LIB, EntryPoint = "CheckExceptionIl2cpp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CheckException();

        private static int Main(string[] args)
        {
            var ptr = GetFuncPointer();
            MonoBind.InitBind(GetFuncPointer());

            if(args.Length > 0)
            {
                var dllPath = args[0];
                Assembly assembly = Assembly.Load(dllPath);
                Type type = assembly.GetType("MonoEntry");
                MethodInfo mi = type.GetMethod("Main");
                var res = mi.Invoke(null, null);
            }

            return 0;
        }
    }
}

