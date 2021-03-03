
using PureScript;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using PureScript.Mono;
public static class MonoBind
{
	public static PureScript_ExceptionTest_set_callback_Type PureScript_ExceptionTest_set_callback;
	public static PureScript_ExceptionTest_get_callback_Type PureScript_ExceptionTest_get_callback;
	public static PureScript_ExceptionTest_NullPointException_Type PureScript_ExceptionTest_NullPointException;
	public static PureScript_ExceptionTest_TestCallBack_Type PureScript_ExceptionTest_TestCallBack;
	public static PureScript_ExceptionTest__ctor_Type PureScript_ExceptionTest__ctor;
	public static PureScript_StartInfo_get_ReloadDllName_Type PureScript_StartInfo_get_ReloadDllName;
	public static PureScript_StartInfo_get_ReloadClassName_Type PureScript_StartInfo_get_ReloadClassName;
	public static PureScript_StartInfo_get_TestMethodName_Type PureScript_StartInfo_get_TestMethodName;
	public static PureScript_StartInfo__ctor_1_Type PureScript_StartInfo__ctor_1;
	public static void InitBind(IntPtr memory)
	{
		int curMemory = 0;
		PureScript_ExceptionTest_set_callback = Marshal.GetDelegateForFunctionPointer<PureScript_ExceptionTest_set_callback_Type>(Marshal.ReadIntPtr(memory, curMemory));
		curMemory += IntPtr.Size;
		PureScript_ExceptionTest_get_callback = Marshal.GetDelegateForFunctionPointer<PureScript_ExceptionTest_get_callback_Type>(Marshal.ReadIntPtr(memory, curMemory));
		curMemory += IntPtr.Size;
		PureScript_ExceptionTest_NullPointException = Marshal.GetDelegateForFunctionPointer<PureScript_ExceptionTest_NullPointException_Type>(Marshal.ReadIntPtr(memory, curMemory));
		curMemory += IntPtr.Size;
		PureScript_ExceptionTest_TestCallBack = Marshal.GetDelegateForFunctionPointer<PureScript_ExceptionTest_TestCallBack_Type>(Marshal.ReadIntPtr(memory, curMemory));
		curMemory += IntPtr.Size;
		PureScript_ExceptionTest__ctor = Marshal.GetDelegateForFunctionPointer<PureScript_ExceptionTest__ctor_Type>(Marshal.ReadIntPtr(memory, curMemory));
		curMemory += IntPtr.Size;
		PureScript_StartInfo_get_ReloadDllName = Marshal.GetDelegateForFunctionPointer<PureScript_StartInfo_get_ReloadDllName_Type>(Marshal.ReadIntPtr(memory, curMemory));
		curMemory += IntPtr.Size;
		PureScript_StartInfo_get_ReloadClassName = Marshal.GetDelegateForFunctionPointer<PureScript_StartInfo_get_ReloadClassName_Type>(Marshal.ReadIntPtr(memory, curMemory));
		curMemory += IntPtr.Size;
		PureScript_StartInfo_get_TestMethodName = Marshal.GetDelegateForFunctionPointer<PureScript_StartInfo_get_TestMethodName_Type>(Marshal.ReadIntPtr(memory, curMemory));
		curMemory += IntPtr.Size;
		PureScript_StartInfo__ctor_1 = Marshal.GetDelegateForFunctionPointer<PureScript_StartInfo__ctor_1_Type>(Marshal.ReadIntPtr(memory, curMemory));
		curMemory += IntPtr.Size;
		Custom.DeSer(memory + curMemory);
	}
}


