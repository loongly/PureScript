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
        public static extern IntPtr GetFuncPointer();

        [DllImport(XMONO_LIB, EntryPoint = "OnExceptionMono", CallingConvention = CallingConvention.Cdecl)]
        public static extern void OnException(string msg);
    }
}

