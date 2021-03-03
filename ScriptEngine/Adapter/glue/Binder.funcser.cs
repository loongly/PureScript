
using PureScript;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using PureScript;
using AOT;
public static unsafe class UnityBind
{
	static readonly PureScript_ExceptionTest_set_callback_Type PureScript_ExceptionTest_set_callbackDelegate = new PureScript_ExceptionTest_set_callback_Type(PureScript_ExceptionTest_set_callback);
	static readonly PureScript_ExceptionTest_get_callback_Type PureScript_ExceptionTest_get_callbackDelegate = new PureScript_ExceptionTest_get_callback_Type(PureScript_ExceptionTest_get_callback);
	static readonly PureScript_ExceptionTest_NullPointException_Type PureScript_ExceptionTest_NullPointExceptionDelegate = new PureScript_ExceptionTest_NullPointException_Type(PureScript_ExceptionTest_NullPointException);
	static readonly PureScript_ExceptionTest_TestCallBack_Type PureScript_ExceptionTest_TestCallBackDelegate = new PureScript_ExceptionTest_TestCallBack_Type(PureScript_ExceptionTest_TestCallBack);
	static readonly PureScript_ExceptionTest__ctor_Type PureScript_ExceptionTest__ctorDelegate = new PureScript_ExceptionTest__ctor_Type(PureScript_ExceptionTest__ctor);
	static readonly PureScript_StartInfo_get_ReloadDllName_Type PureScript_StartInfo_get_ReloadDllNameDelegate = new PureScript_StartInfo_get_ReloadDllName_Type(PureScript_StartInfo_get_ReloadDllName);
	static readonly PureScript_StartInfo_get_ReloadClassName_Type PureScript_StartInfo_get_ReloadClassNameDelegate = new PureScript_StartInfo_get_ReloadClassName_Type(PureScript_StartInfo_get_ReloadClassName);
	static readonly PureScript_StartInfo_get_TestMethodName_Type PureScript_StartInfo_get_TestMethodNameDelegate = new PureScript_StartInfo_get_TestMethodName_Type(PureScript_StartInfo_get_TestMethodName);
	static readonly PureScript_StartInfo__ctor_1_Type PureScript_StartInfo__ctor_1Delegate = new PureScript_StartInfo__ctor_1_Type(PureScript_StartInfo__ctor_1);
	public static IntPtr BindFunc()
	{
		IntPtr memory = Marshal.AllocHGlobal(8192*8);
		int curMemory = 0;;
		Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(PureScript_ExceptionTest_set_callbackDelegate));
		curMemory += IntPtr.Size;
		Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(PureScript_ExceptionTest_get_callbackDelegate));
		curMemory += IntPtr.Size;
		Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(PureScript_ExceptionTest_NullPointExceptionDelegate));
		curMemory += IntPtr.Size;
		Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(PureScript_ExceptionTest_TestCallBackDelegate));
		curMemory += IntPtr.Size;
		Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(PureScript_ExceptionTest__ctorDelegate));
		curMemory += IntPtr.Size;
		Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(PureScript_StartInfo_get_ReloadDllNameDelegate));
		curMemory += IntPtr.Size;
		Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(PureScript_StartInfo_get_ReloadClassNameDelegate));
		curMemory += IntPtr.Size;
		Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(PureScript_StartInfo_get_TestMethodNameDelegate));
		curMemory += IntPtr.Size;
		Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(PureScript_StartInfo__ctor_1Delegate));
		curMemory += IntPtr.Size;
		Custom.Ser(memory + curMemory);
		return memory;
	}
	
			static Delegate5523eb28 ExceptionTest_callback;
			static void OnExceptionTest_callback(this global::PureScript.ExceptionTest arg0)
			{
				var arg0_h = ObjectStore.Store(arg0);
				ExceptionTest_callback(arg0_h);
				ScriptEngine.CheckException();
			}
	[MonoPInvokeCallback(typeof(PureScript_ExceptionTest_set_callback_Type))]
	static void PureScript_ExceptionTest_set_callback (int thiz_h, IntPtr value_p)
	{
		Exception __e = null;
		try
		{
			var thizObj = ObjectStore.Get<global::PureScript.ExceptionTest>(thiz_h);
			ExceptionTest_callback = Marshal.GetDelegateForFunctionPointer<Delegate5523eb28>(value_p);
			thizObj.callback = thizObj.OnExceptionTest_callback;
		}
		catch(Exception e)
		{
			__e = e;
		}
		if(__e != null)
		ScriptEngine.OnException(__e.ToString());
	}
	
	[MonoPInvokeCallback(typeof(PureScript_ExceptionTest_get_callback_Type))]
	static IntPtr PureScript_ExceptionTest_get_callback (int thiz_h)
	{
		Exception __e = null;
		try
		{
			var thizObj = ObjectStore.Get<global::PureScript.ExceptionTest>(thiz_h);
			var _value = thizObj.callback;
			var _value_p = Marshal.GetFunctionPointerForDelegate(_value);
			return _value_p;
		}
		catch(Exception e)
		{
			__e = e;
		}
		if(__e != null)
		ScriptEngine.OnException(__e.ToString());
		return default(IntPtr);
	}
	
	[MonoPInvokeCallback(typeof(PureScript_ExceptionTest_NullPointException_Type))]
	static void PureScript_ExceptionTest_NullPointException (int thiz_h)
	{
		Exception __e = null;
		try
		{
			var thizObj = ObjectStore.Get<global::PureScript.ExceptionTest>(thiz_h);
			thizObj.NullPointException();
		}
		catch(Exception e)
		{
			__e = e;
		}
		if(__e != null)
		ScriptEngine.OnException(__e.ToString());
	}
	
	[MonoPInvokeCallback(typeof(PureScript_ExceptionTest_TestCallBack_Type))]
	static void PureScript_ExceptionTest_TestCallBack (int thiz_h)
	{
		Exception __e = null;
		try
		{
			var thizObj = ObjectStore.Get<global::PureScript.ExceptionTest>(thiz_h);
			thizObj.TestCallBack();
		}
		catch(Exception e)
		{
			__e = e;
		}
		if(__e != null)
		ScriptEngine.OnException(__e.ToString());
	}
	
	[MonoPInvokeCallback(typeof(PureScript_ExceptionTest__ctor_Type))]
	static int PureScript_ExceptionTest__ctor ()
	{
		Exception __e = null;
		try
		{
			var _value = new global::PureScript.ExceptionTest();
			var _valueHandle = ObjectStore.Store(_value);
			return _valueHandle;
		}
		catch(Exception e)
		{
			__e = e;
		}
		if(__e != null)
		ScriptEngine.OnException(__e.ToString());
		return default(int);
	}
	
	[MonoPInvokeCallback(typeof(PureScript_StartInfo_get_ReloadDllName_Type))]
	static System.String PureScript_StartInfo_get_ReloadDllName ()
	{
		Exception __e = null;
		try
		{
			var _value = PureScript.StartInfo.ReloadDllName;
			return _value;
		}
		catch(Exception e)
		{
			__e = e;
		}
		if(__e != null)
		ScriptEngine.OnException(__e.ToString());
		return default(System.String);
	}
	
	[MonoPInvokeCallback(typeof(PureScript_StartInfo_get_ReloadClassName_Type))]
	static System.String PureScript_StartInfo_get_ReloadClassName ()
	{
		Exception __e = null;
		try
		{
			var _value = PureScript.StartInfo.ReloadClassName;
			return _value;
		}
		catch(Exception e)
		{
			__e = e;
		}
		if(__e != null)
		ScriptEngine.OnException(__e.ToString());
		return default(System.String);
	}
	
	[MonoPInvokeCallback(typeof(PureScript_StartInfo_get_TestMethodName_Type))]
	static System.String PureScript_StartInfo_get_TestMethodName ()
	{
		Exception __e = null;
		try
		{
			var _value = PureScript.StartInfo.TestMethodName;
			return _value;
		}
		catch(Exception e)
		{
			__e = e;
		}
		if(__e != null)
		ScriptEngine.OnException(__e.ToString());
		return default(System.String);
	}
	
	[MonoPInvokeCallback(typeof(PureScript_StartInfo__ctor_1_Type))]
	static int PureScript_StartInfo__ctor_1 ()
	{
		Exception __e = null;
		try
		{
			var _value = new global::PureScript.StartInfo();
			var _valueHandle = ObjectStore.Store(_value);
			return _valueHandle;
		}
		catch(Exception e)
		{
			__e = e;
		}
		if(__e != null)
		ScriptEngine.OnException(__e.ToString());
		return default(int);
	}
}

