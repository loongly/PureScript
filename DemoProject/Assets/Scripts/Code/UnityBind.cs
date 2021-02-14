using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using XMono;

public class UnityBind
{

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void UnityEngineDebugMethodLogSystemObjectDelegateType(int messageHandle);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegateType(int typeint);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int UnityEngineComponentPropertyGetTransformDelegateType(int thisHandle);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void UnityEngineTransformPropertySetPositionDelegateType(int thisHandle, ref Vector3 value);


    static readonly UnityEngineDebugMethodLogSystemObjectDelegateType UnityEngineDebugMethodLogSystemObjectDelegate = new UnityEngineDebugMethodLogSystemObjectDelegateType(UnityEngineDebugMethodLogSystemObject);
    static readonly UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegateType UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegate = new UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegateType(UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveType);
    static readonly UnityEngineComponentPropertyGetTransformDelegateType UnityEngineComponentPropertyGetTransformDelegate = new UnityEngineComponentPropertyGetTransformDelegateType(UnityEngineComponentPropertyGetTransform);
    static readonly UnityEngineTransformPropertySetPositionDelegateType UnityEngineTransformPropertySetPositionDelegate = new UnityEngineTransformPropertySetPositionDelegateType(UnityEngineTransformPropertySetPosition);

    public static void BindXMono()
    {
        int memorySize = 1024;
        IntPtr memory = Marshal.AllocHGlobal(memorySize);
        // Pass parameters through 'memory'
        int curMemory = 0;

        Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineDebugMethodLogSystemObjectDelegate));
        curMemory += IntPtr.Size;
        Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegate));
        curMemory += IntPtr.Size;
        Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineComponentPropertyGetTransformDelegate));
        curMemory += IntPtr.Size;
        Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertySetPositionDelegate));
        curMemory += IntPtr.Size;

        ScriptEngine.SetFuncPointer(memory);
    }


    [MonoPInvokeCallback(typeof(UnityEngineDebugMethodLogSystemObjectDelegateType))]
    static void UnityEngineDebugMethodLogSystemObject(int messageHandle)
    {
        try
        {
            UnityEngine.Debug.LogError("== "+ messageHandle);
        }
        catch (System.NullReferenceException ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
    }


    [MonoPInvokeCallback(typeof(UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegateType))]
    static int UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveType(int typeint)
    {
        try
        {
            UnityEngine.PrimitiveType type = (UnityEngine.PrimitiveType)typeint;
            var returnValue = UnityEngine.GameObject.CreatePrimitive(type);
            return ObjectStore.GetHandle(returnValue);
        }
        catch (System.NullReferenceException ex)
        {
            UnityEngine.Debug.LogException(ex);
            return default(int);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
            return default(int);
        }
    }

    [MonoPInvokeCallback(typeof(UnityEngineComponentPropertyGetTransformDelegateType))]
    static int UnityEngineComponentPropertyGetTransform(int thisHandle)
    {
        try
        {
            var thiz = (GameObject)ObjectStore.Get(thisHandle);
            var returnValue = thiz.transform;
            return ObjectStore.GetHandle(returnValue);
        }
        catch (System.NullReferenceException ex)
        {
            UnityEngine.Debug.LogException(ex);
            return default(int);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
            return default(int);
        }
    }

    [MonoPInvokeCallback(typeof(UnityEngineTransformPropertySetPositionDelegateType))]
    static void UnityEngineTransformPropertySetPosition(int thisHandle, ref Vector3 value)
    {
        try
        {
            var thiz = (UnityEngine.Transform)ObjectStore.Get(thisHandle);
            Vector3 vector = new Vector3(value.x, value.y, value.z);
            thiz.position = vector;
        }
        catch (System.NullReferenceException ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
    }
}
