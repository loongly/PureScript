using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System;

namespace PureScript
{
    public class ScriptEngine
    {

        const string XMONO_LIB = "__Internal";

        //const string XMONO_LIB = "ScriptEngine";


        [DllImport(XMONO_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetFuncPointer();

        [DllImport(XMONO_LIB, EntryPoint = "OnExceptionMono", CallingConvention = CallingConvention.Cdecl)]
        public static extern void OnException(string msg);
        [DllImport(XMONO_LIB, EntryPoint = "CheckExceptionIl2cpp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CheckException();
    }
}

