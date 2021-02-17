using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PureScript
{
    public class ScriptEngine
    {
        const string XMONO_LIB = "ScriptEngine";

        [DllImport(XMONO_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetupMono([In()] [MarshalAs(UnmanagedType.LPStr)]string bundleDir, [In()] [MarshalAs(UnmanagedType.LPStr)] string exeName);
        [DllImport(XMONO_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseMono();

        [DllImport(XMONO_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFuncPointer(IntPtr ptr);

        [DllImport(XMONO_LIB, EntryPoint = "OnExceptionIl2cpp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void OnException(string msg);
    }
}

