using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScriptEngine
{
    static IntPtr funcPtr;
#if TEST_MONO
    /*public static void SetupMono(string bundleDir, string exeName)
    {
        FuncTest.TestMain(funcPtr);
    }
    public static void CloseMono() { }

    public static void SetFuncPointer(IntPtr ptr)
    {
        funcPtr = ptr;
    }*/
#else

    const string XMONO_LIB = "ScriptEngine";
    [DllImport(XMONO_LIB, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetupMono([In()] [MarshalAs(UnmanagedType.LPStr)]string bundleDir, [In()] [MarshalAs(UnmanagedType.LPStr)] string exeName);
    [DllImport(XMONO_LIB, CallingConvention = CallingConvention.Cdecl)]
    public static extern void CloseMono();

    [DllImport(XMONO_LIB, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetFuncPointer(IntPtr ptr);
#endif  
}
